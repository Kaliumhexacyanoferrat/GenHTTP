using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ApiBrowsing.Common;

public class BrowserHandlerBuilder(string resourceRoot) : IHandlerBuilder<BrowserHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private string? _Url;

    public BrowserHandlerBuilder Url(string url)
    {
        _Url = url;
        return this;
    }

    public BrowserHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_Concerns, new BrowserHandler(resourceRoot, _Url));

}
