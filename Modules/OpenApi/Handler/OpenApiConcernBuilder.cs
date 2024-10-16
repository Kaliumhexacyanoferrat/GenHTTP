using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Discovery;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiConcernBuilder : IConcernBuilder
{

    #region Get-/Setters

    private ApiDiscoveryRegistry Discovery { get; }

    #endregion

    #region Initialization

    public OpenApiConcernBuilder(ApiDiscoveryRegistry registry)
    {
        Discovery = registry;
    }

    #endregion

    #region Functionality

    public OpenApiConcernBuilder Title(string title)
    {
        // todo
        return this;
    }

    public OpenApiConcernBuilder Version(string version)
    {
        // todo
        return this;
    }

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
    {
        return new OpenApiConcern(parent, contentFactory, Discovery);
    }

    #endregion

}
