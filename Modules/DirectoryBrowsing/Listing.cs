using System.IO;
using System.Reflection;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.DirectoryBrowsing.Provider;

namespace GenHTTP.Modules.DirectoryBrowsing
{

    public static class Listing
    {

        public static ListingRouterBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

        public static ListingRouterBuilder From(IResourceTree tree) => new ListingRouterBuilder().Tree(tree);

    }

}
