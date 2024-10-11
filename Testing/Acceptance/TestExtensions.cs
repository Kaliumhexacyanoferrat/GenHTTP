using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using GenHTTP.Api.Content;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance;

public static class TestExtensions
{

    public static IHandlerBuilder<HandlerBuilder> Wrap(this IHandler handler) => new HandlerBuilder(handler);

    public static string? GetETag(this HttpResponseMessage response) => response.GetHeader("ETag");

    public static async Task<HashSet<string>> GetSitemap(this HttpResponseMessage response)
    {
        var content = await response.GetContentAsync();

        var sitemap = XDocument.Parse(content);

        var namespaces = new XmlNamespaceManager(new NameTable());

        namespaces.AddNamespace("n", "http://www.sitemaps.org/schemas/sitemap/0.9");

        return sitemap.Root?.XPathSelectElements("//n:loc", namespaces)
                      .Select(x => new Uri(x.Value).AbsolutePath)
                      .ToHashSet() ?? new HashSet<string>();
    }

    public static DateTime WithoutMS(this DateTime date) => new(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
}
