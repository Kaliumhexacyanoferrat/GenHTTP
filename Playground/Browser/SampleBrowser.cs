using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Browser;

/// <summary>
/// Serves all samples below their own path, together with a page
/// at the root that lists them and allows to call them.
/// </summary>
internal static class SampleBrowser
{
    private static readonly ContentType SourceType = new("text/plain; charset=utf-8");

    internal static IHandlerBuilder Create()
    {
        var sources = Layout.Create();

        foreach (var sample in Catalog.Samples)
        {
            sources.Add(sample.Source, Content.From(Resource.FromAssembly($"Source.{sample.Source}").Type(SourceType)));
        }

        var playground = Layout.Create()
                               .Add("samples", Inline.Create().Get(() => Catalog.Samples))
                               .Add("source", sources);

        var app = Layout.Create()
                        .Index(Content.From(Resource.FromAssembly("Browser.Index.html")))
                        .Add("playground", playground);

        foreach (var sample in Catalog.Samples)
        {
            app.Add(sample.Segment, sample.Handler);
        }

        return app;
    }

}
