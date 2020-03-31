using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core.Sitemaps;

namespace GenHTTP.Modules.Core
{

    public static class Sitemap
    {

        public static SitemapRouterBuilder Create() => new SitemapRouterBuilder();

        public static SitemapRouterBuilder From(IRouter content) => new SitemapRouterBuilder().Content(content);

    }

}
