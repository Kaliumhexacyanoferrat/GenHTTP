using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.StaticWebsites.Provider;

namespace GenHTTP.Modules.StaticWebsites
{

    public static class StaticWebsite
    {

        public static StaticWebsiteBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

        public static StaticWebsiteBuilder From(IResourceTree tree) => new StaticWebsiteBuilder().Tree(tree);

    }

}
