using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.OpenApi.Discovery;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiHandler : IHandler
{

    #region Get-/Setters

    public IHandler Parent { get; }

    private OpenApiDocument Document { get; }

    private ApiDiscoveryRegistry? Discovery { get; }

    private OpenApiSpecVersion Version { get; }

    #endregion

    #region Initialization

    public OpenApiHandler(IHandler parent, OpenApiDocument document, OpenApiSpecVersion version, ApiDiscoveryRegistry? discovery)
    {
        Parent = parent;

        Document = document;
        Version = version;
        Discovery = discovery;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => new();

    public ValueTask<IResponse?> HandleAsync(IRequest request)
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
                        "application/json" or "application/application/vnd.oai.openapi+json" => GetDocument(request, OpenApiFormat.Json),
                        "application/yaml" or "application/application/vnd.oai.openapi+yaml" => GetDocument(request, OpenApiFormat.Yaml),
                        _ => throw new ProviderException(ResponseStatus.BadRequest, $"Generating API specifications of format '{accept}' is not supported")
                    };
                }
                else
                {
                    response = GetDocument(request, OpenApiFormat.Json);
                }

                response.Headers.Add("Vary", "Accept");

                return new(response);
            }
            else if (string.Compare(path, "openapi.json", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new(GetDocument(request, OpenApiFormat.Json));
            }
            else if (string.Compare(path, "openapi.yaml", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(path, "openapi.yml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new (GetDocument(request, OpenApiFormat.Yaml));
            }
        }

        return new();
    }

    private IResponse GetDocument(IRequest request, OpenApiFormat format)
    {
        var document = (Discovery != null) ? Discover(Discovery) : Document;

        var content = new OpenApiContent(document, format, Version);

        var contentType = (format == OpenApiFormat.Json) ? FlexibleContentType.Get(ContentType.ApplicationJson) : FlexibleContentType.Get(ContentType.ApplicationYaml);

        return request.Respond()
                      .Content(content)
                      .Type(contentType)
                      .Build();
    }

    private OpenApiDocument Discover(ApiDiscoveryRegistry registry)
    {
        var document = new OpenApiDocument(Document);

        var parent = GetParent();

        registry.Explore(parent, []);

        return document;
    }

    private IHandler GetParent()
    {
        var parent = Parent;

        while (parent is IConcern)
        {
            parent = parent.Parent;
        }

        return parent;
    }

    #endregion

}
