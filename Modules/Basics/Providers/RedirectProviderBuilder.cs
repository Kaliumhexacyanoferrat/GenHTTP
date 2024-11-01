using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Basics.Providers;

public sealed class RedirectProviderBuilder : IHandlerBuilder<RedirectProviderBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private string? _Location;
    private bool _Temporary;

    #region Functionality

    public RedirectProviderBuilder Location(string location)
    {
        _Location = location;
        return this;
    }

    public RedirectProviderBuilder Mode(bool temporary)
    {
        _Temporary = temporary;
        return this;
    }

    public RedirectProviderBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_Location is null)
        {
            throw new BuilderMissingPropertyException("Location");
        }

        return Concerns.Chain(_Concerns, new RedirectProvider(_Location, _Temporary));
    }

    #endregion

}
