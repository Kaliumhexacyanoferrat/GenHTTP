using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ApiBrowsing.Common;

public class BrowserHandlerBuilder(string resourceRoot, string title) : IHandlerBuilder<BrowserHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private string? _Url;

    private string _Title = title;

    public BrowserHandlerBuilder Url(string url)
    {
        _Url = url;
        return this;
    }

    public BrowserHandlerBuilder Title(string title)
    {
        _Title = title;
        return this;
    }

    public BrowserHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var meta = new BrowserMetaData(_Url, _Title);

        return Concerns.Chain(_Concerns, new BrowserHandler(resourceRoot, meta));
    }

}
