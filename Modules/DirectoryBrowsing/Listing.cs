using System.IO;
using System.Reflection;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.DirectoryBrowsing.Provider;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing
{

    public static class Listing
    {

        public static ListingRouterBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

        public static ListingRouterBuilder From(IResourceTree tree) => new ListingRouterBuilder().Tree(tree);

        public static ListingRouterBuilder FromAssembly(string root) => FromAssembly(Assembly.GetCallingAssembly(), root);

        public static ListingRouterBuilder FromAssembly(Assembly source, string root) => From(ResourceTree.FromAssembly(source, root));

        public static ListingRouterBuilder FromDirectory(string directory) => From(ResourceTree.FromDirectory(directory));

        public static ListingRouterBuilder FromDirectory(DirectoryInfo directory) => From(ResourceTree.FromDirectory(directory));

    }

}
