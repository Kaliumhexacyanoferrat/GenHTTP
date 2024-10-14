using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Discovery;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiHandlerBuilder : IHandlerBuilder<OpenApiHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private ApiDiscoveryRegistry? _Discovery;

    private bool _Cache = true;

    private OpenApiSpecVersion _Specfication = OpenApiSpecVersion.OpenApi3_0;

    #region Get-/Setters

    public OpenApiDocument Document { get; }

    #endregion

    #region Initialization

    public OpenApiHandlerBuilder(OpenApiDocument? document = null)
    {
        Document = document ?? new OpenApiDocument();
    }

    #endregion

    #region Functionality

    public OpenApiHandlerBuilder Specification(OpenApiSpecVersion version)
    {
        _Specfication = version;
        return this;
    }

    public OpenApiHandlerBuilder Discovery(ApiDiscoveryRegistryBuilder discovery) => Discovery(discovery.Build());

    public OpenApiHandlerBuilder Discovery(ApiDiscoveryRegistry discovery)
    {
        _Discovery = discovery;
        return this;
    }

    public OpenApiHandlerBuilder Cache(bool enabled)
    {
        _Cache = enabled;
        return this;
    }

    public OpenApiHandlerBuilder Title(string title)
    {
        (Document.Info ??= new()).Title = title;
        return this;
    }

    public OpenApiHandlerBuilder Version(string version)
    {
        (Document.Info ??= new()).Version = version;
        return this;
    }

    public OpenApiHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        return Concerns.Chain(parent, _Concerns, p => new OpenApiHandler(p, Document, _Specfication, _Discovery));
    }

    #endregion

}
