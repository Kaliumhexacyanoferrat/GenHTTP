using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ContentProviderBuilder : IHandlerBuilder<ContentProviderBuilder>
{

    private readonly List<IConcernBuilder> _Concerns = new();
    private IResource? _ResourceProvider;

    #region Functionality

    public ContentProviderBuilder Resource(IResource resource)
    {
        _ResourceProvider = resource;
        return this;
    }

    public ContentProviderBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        var resource = _ResourceProvider ?? throw new BuilderMissingPropertyException("resourceProvider");

        return Concerns.Chain(parent, _Concerns, p => new ContentProvider(p, resource));
    }

    #endregion

}
