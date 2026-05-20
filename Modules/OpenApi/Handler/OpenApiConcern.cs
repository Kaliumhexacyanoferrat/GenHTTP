using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.OpenApi.Discovery;

using NJsonSchema;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

internal record ReturnDocument(OpenApiDocument Document, ulong Checksum);

public sealed class OpenApiConcern : IConcern
{
    private static readonly ReadOnlyMemory<byte> OpenApiPath = "openapi"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> OpenApiJsonPath = "openapi.json"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> OpenApiYamlPath = "openapi.yaml"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> OpenApiYmlPath = "openapi.yml"u8.ToArray();

    private ReturnDocument? _cached;

    #region Get-/Setters

    public IHandler Content { get; }

    private ApiDiscoveryRegistry Discovery { get; }

    private bool EnableCaching { get; }

    private Action<IRequest, OpenApiDocument> PostProcessor { get; }

    #endregion

    #region Initialization

    public OpenApiConcern(IHandler content, ApiDiscoveryRegistry discovery, bool enableCaching, Action<IRequest, OpenApiDocument> postProcessor)
    {
        Content = content;

        Discovery = discovery;
        EnableCaching = enableCaching;
        PostProcessor = postProcessor;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method == RequestMethod.Get || request.Header.Method == RequestMethod.Head)
        {
            var path = request.Header.Target.Current;

            if (path != null)
            {
                var pathSpan = path.Value.Encoded.Span;

                if (pathSpan.SequenceEqual(OpenApiPath.Span))
                {
                    IResponse response;

                    var accept = request.Header.Headers.GetEntry("Accept");

                    if (accept != null)
                    {
                        response = accept.ToLowerInvariant() switch
                        {
                            "application/json" or "application/application/vnd.oai.openapi+json" => await GetDocumentAsync(request, OpenApiFormat.Json),
                            "application/yaml" or "application/application/vnd.oai.openapi+yaml" => await GetDocumentAsync(request, OpenApiFormat.Yaml),
                            _ => throw new ProviderException(ResponseStatus.BadRequest, $"Generating API specifications of format '{accept}' is not supported")
                        };
                    }
                    else
                    {
                        response = await GetDocumentAsync(request, OpenApiFormat.Json);
                    }

                    response.Rebuild().Header("Vary", "Accept");

                    return response;
                }
                if (pathSpan.SequenceEqual(OpenApiJsonPath.Span))
                {
                    return await GetDocumentAsync(request, OpenApiFormat.Json);
                }
                if (pathSpan.SequenceEqual(OpenApiYamlPath.Span) || pathSpan.SequenceEqual(OpenApiYamlPath.Span))
                {
                    return await GetDocumentAsync(request, OpenApiFormat.Yaml);
                }
            }
        }

        return await Content.HandleAsync(request);
    }

    private async ValueTask<IResponse> GetDocumentAsync(IRequest request, OpenApiFormat format)
    {
        var document = await DiscoverAsync(request, Discovery);

        return request.Respond()
                      .Content(new OpenApiContent(document, format))
                      .Build();
    }

    private async ValueTask<ReturnDocument> DiscoverAsync(IRequest request, ApiDiscoveryRegistry registry)
    {
        if (EnableCaching && _cached != null)
        {
            return _cached;
        }

        var document = new OpenApiDocument();

        var schemata = new SchemaManager(document);

        document.SchemaType = SchemaType.OpenApi3;

        var path = request.Header.Target.AsString(decode: false);

        var host = request.Header.Headers.GetEntry("Host");

        if (host != null)
        {
            document.Servers.Add(new OpenApiServer
            {
                Url = (request.EndPoint.Secure ? "https://" : "http://") + host + path[..path.LastIndexOf('/')]
            });
        }

        await registry.ExploreAsync(request, Content, [], document, schemata);

        PostProcessor.Invoke(request, document);

        if (EnableCaching)
        {
            _cached = new(document, GetChecksum(document));
            return _cached;
        }

        return new(document, GetChecksum(document));
    }

    private static ulong GetChecksum(OpenApiDocument document) => (ulong)document.ToJson().GetHashCode();

    #endregion

}
