using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.OpenApi.Discovery;
using NJsonSchema;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

internal record ReturnDocument(OpenApiDocument Document, ulong Checksum);

public sealed class OpenApiConcern : IConcern
{
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
        var path = request.Target.Current?.Original;

        if (request.Method == RequestMethod.Get || request.Method == RequestMethod.Head)
        {
            if (string.Compare(path, "openapi", StringComparison.OrdinalIgnoreCase) == 0)
            {
                IResponse response;

                if (request.Headers.TryGetValue("Accept", out var accept))
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

                response.Headers.Add("Vary", "Accept");

                return response;
            }
            if (string.Compare(path, "openapi.json", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return await GetDocumentAsync(request, OpenApiFormat.Json);
            }
            if (string.Compare(path, "openapi.yaml", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(path, "openapi.yml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return await GetDocumentAsync(request, OpenApiFormat.Yaml);
            }
        }

        return await Content.HandleAsync(request);
    }

    private async ValueTask<IResponse> GetDocumentAsync(IRequest request, OpenApiFormat format)
    {
        var document = await DiscoverAsync(request, Discovery);

        var content = new OpenApiContent(document, format);

        var contentType = format == OpenApiFormat.Json ? FlexibleContentType.Get(ContentType.ApplicationJson) : FlexibleContentType.Get(ContentType.ApplicationYaml);

        return request.Respond()
                      .Content(content)
                      .Type(contentType)
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

        var path = request.Target.Path.ToString();

        if (request.Host != null)
        {
            document.Servers.Add(new OpenApiServer
            {
                Url = (request.EndPoint.Secure ? "https://" : "http://") + request.Host + path[..path.LastIndexOf('/')]
            });
        }

        await registry.ExploreAsync(request, Content, [], document, schemata);

        PostProcessor.Invoke(request, document);

        if (EnableCaching)
        {
            _cached = new (document, GetChecksum(document));
            return _cached;
        }

        return new (document, GetChecksum(document));
    }

    private static ulong GetChecksum(OpenApiDocument document) => (ulong)document.ToJson().GetHashCode();

    #endregion

}
