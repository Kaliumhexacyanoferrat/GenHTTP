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

    public OpenApiConcernBuilder Title(string title)
    {
        _Title = title;
        return this;
    }

    public OpenApiConcernBuilder Version(string version)
    {
        _Version = version;
        return this;
    }

    public OpenApiConcernBuilder Caching(bool enabled)
    {
        _Caching = enabled;
        return this;
    }

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
