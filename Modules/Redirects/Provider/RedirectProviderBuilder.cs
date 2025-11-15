using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Redirects.Provider;

public sealed class RedirectProviderBuilder : IHandlerBuilder<RedirectProviderBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private string? _location;
    private bool _temporary;

    #region Functionality

    public RedirectProviderBuilder Location(string location)
    {
        _location = location;
        return this;
    }

    public RedirectProviderBuilder Mode(bool temporary)
    {
        _temporary = temporary;
        return this;
    }

    public RedirectProviderBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_location is null)
        {
            throw new BuilderMissingPropertyException("Location");
        }

        return Concerns.Chain(_concerns, new RedirectProvider(_location, _temporary));
    }

    #endregion

}
