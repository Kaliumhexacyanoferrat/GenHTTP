using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Discovery;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiConcernBuilder : IConcernBuilder
{
    private ApiDiscoveryRegistry? _Discovery;

    private bool _Cache = true;

    private OpenApiSpecVersion _Specfication = OpenApiSpecVersion.OpenApi3_0;

    #region Get-/Setters

    public OpenApiDocument Document { get; }

    #endregion

    #region Initialization

    public OpenApiConcernBuilder(OpenApiDocument? document = null)
    {
        Document = document ?? new OpenApiDocument();
    }

    #endregion

    #region Functionality

    public OpenApiConcernBuilder Specification(OpenApiSpecVersion version)
    {
        _Specfication = version;
        return this;
    }

    public OpenApiConcernBuilder Discovery(ApiDiscoveryRegistryBuilder discovery) => Discovery(discovery.Build());

    public OpenApiConcernBuilder Discovery(ApiDiscoveryRegistry discovery)
    {
        _Discovery = discovery;
        return this;
    }

    public OpenApiConcernBuilder Cache(bool enabled)
    {
        _Cache = enabled;
        return this;
    }

    public OpenApiConcernBuilder Title(string title)
    {
        (Document.Info ??= new()).Title = title;
        return this;
    }

    public OpenApiConcernBuilder Version(string version)
    {
        (Document.Info ??= new()).Version = version;
        return this;
    }

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
    {
        return new OpenApiConcern(parent, contentFactory, Document, _Specfication, _Discovery);
    }

    #endregion

}
