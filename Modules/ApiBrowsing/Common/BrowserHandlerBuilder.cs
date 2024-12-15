using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ApiBrowsing.Common;

public class BrowserHandlerBuilder(string resourceRoot, string title) : IHandlerBuilder<BrowserHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private string? _Url;

    private string _Title = title;

    /// <summary>
    /// Sets the URL of the Open API definition to be consumed (defaults to "../openapi.json").
    /// Should be relative to avoid issues with CORS etc.
    /// </summary>
    /// <param name="url">The URL the application will fetch the Open API definition from</param>
    public BrowserHandlerBuilder Url(string url)
    {
        _Url = url;
        return this;
    }

    /// <summary>
    /// Sets the title of the application that will be rendered by the browser (e.g. the title of the tab).
    /// </summary>
    /// <param name="title">The title of the application to be set</param>
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
