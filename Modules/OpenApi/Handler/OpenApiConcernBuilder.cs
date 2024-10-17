using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.OpenApi.Discovery;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiConcernBuilder : IConcernBuilder
{
    private bool _Caching = true;

    private Action<IRequest, OpenApiDocument>? _PostProcessor;

    private string? _Title, _Version;

    #region Initialization

    public OpenApiConcernBuilder(ApiDiscoveryRegistry registry)
    {
        Discovery = registry;
    }

    #endregion

    #region Get-/Setters

    private ApiDiscoveryRegistry Discovery { get; }

    #endregion

    #region Functionality

    /// <summary>
    /// Sets the title of the OpenAPI specification.
    /// </summary>
    /// <param name="title">The title of the API</param>
    public OpenApiConcernBuilder Title(string title)
    {
        _Title = title;
        return this;
    }

    /// <summary>
    /// Sets the version of the described API.
    /// </summary>
    /// <param name="version">The version of the API</param>
    public OpenApiConcernBuilder Version(string version)
    {
        _Version = version;
        return this;
    }

    /// <summary>
    /// Specifies, whether the generated OpenAPI specification should
    /// get cached on first request, so it is no re-generated on every request.
    /// </summary>
    /// <param name="enabled">Whether to use caching or not</param>
    public OpenApiConcernBuilder Caching(bool enabled)
    {
        _Caching = enabled;
        return this;
    }

    /// <summary>
    /// Registers a function that will be called when an OpenAPI document has been
    /// generated, directly before it is served to the client.
    /// </summary>
    /// <param name="action">The method to be invoked to adjust the generated document</param>
    public OpenApiConcernBuilder PostProcessor(Action<IRequest, OpenApiDocument> action)
    {
        _PostProcessor = action;
        return this;
    }

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory) => new OpenApiConcern(parent, contentFactory, Discovery, _Caching, DoPostProcessing);

    private void DoPostProcessing(IRequest request, OpenApiDocument document)
    {
        if (_Title != null)
        {
            document.Info.Title = _Title;
        }

        if (_Version != null)
        {
            document.Info.Version = _Version;
        }

        _PostProcessor?.Invoke(request, document);
    }

    #endregion

}
