using GenHTTP.Modules.Sitemaps.Provider;

namespace GenHTTP.Modules.Sitemaps
{

    public static class Sitemap
    {

        public const string FILE_NAME = "sitemap.xml";

        public static SitemapProviderBuilder Create() => new SitemapProviderBuilder();

    }

}
