using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.OpenApi.Discovery;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiConcernBuilder : IConcernBuilder
{
    private bool _caching = true;

    private Action<IRequest, OpenApiDocument>? _postProcessor;

    private string? _title, _version;

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
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the version of the described API.
    /// </summary>
    /// <param name="version">The version of the API</param>
    public OpenApiConcernBuilder Version(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Specifies, whether the generated OpenAPI specification should
    /// get cached on first request, so it is no re-generated on every request.
    /// </summary>
    /// <param name="enabled">Whether to use caching or not</param>
    public OpenApiConcernBuilder Caching(bool enabled)
    {
        _caching = enabled;
        return this;
    }

    /// <summary>
    /// Registers a function that will be called when an OpenAPI document has been
    /// generated, directly before it is served to the client.
    /// </summary>
    /// <param name="action">The method to be invoked to adjust the generated document</param>
    public OpenApiConcernBuilder PostProcessor(Action<IRequest, OpenApiDocument> action)
    {
        _postProcessor = action;
        return this;
    }

    public IConcern Build(IHandler content) => new OpenApiConcern(content, Discovery, _caching, DoPostProcessing);

    private void DoPostProcessing(IRequest request, OpenApiDocument document)
    {
        if (_title != null)
        {
            document.Info.Title = _title;
        }

        if (_version != null)
        {
            document.Info.Version = _version;
        }

        _postProcessor?.Invoke(request, document);
    }

    #endregion

}
