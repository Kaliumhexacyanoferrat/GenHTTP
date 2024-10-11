using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.StaticWebsites.Provider;

namespace GenHTTP.Modules.StaticWebsites;

public static class StaticWebsite
{

    /// <summary>
    ///     Creates a static website from the given resource tree which
    ///     will automatically serve index files (index.html, index.htm)
    ///     and provide a sitemap and robots instruction file.
    /// </summary>
    /// <param name="tree">The resource to generate the application from</param>
    public static StaticWebsiteBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

    /// <summary>
    ///     Creates a static website from the given resource tree which
    ///     will automatically serve index files (index.html, index.htm)
    ///     and provide a sitemap and robots instruction file.
    /// </summary>
    /// <param name="tree">The resource to generate the application from</param>
    public static StaticWebsiteBuilder From(IResourceTree tree) => new StaticWebsiteBuilder().Tree(tree);
}
