using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ContentProviderBuilder : IHandlerBuilder<ContentProviderBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IResource? _resourceProvider;

    #region Functionality

    public ContentProviderBuilder Resource(IResource resource)
    {
        _resourceProvider = resource;
        return this;
    }

    public ContentProviderBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var resource = _resourceProvider ?? throw new BuilderMissingPropertyException("resourceProvider");

        return Concerns.Chain(_concerns,  new ContentProvider(resource));
    }

    #endregion

}
