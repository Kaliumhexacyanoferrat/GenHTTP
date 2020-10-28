using System.IO;
using System.Reflection;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.SinglePageApplications.Provider;

namespace GenHTTP.Modules.SinglePageApplications
{

    public static class SinglePageApplication
    {

        public static SinglePageBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

        public static SinglePageBuilder From(IResourceTree tree) => new SinglePageBuilder().Tree(tree);

        public static SinglePageBuilder FromAssembly(string root) => FromAssembly(Assembly.GetCallingAssembly(), root);

        public static SinglePageBuilder FromAssembly(Assembly source, string root) => From(ResourceTree.FromAssembly(source, root));

        public static SinglePageBuilder FromDirectory(string directory) => From(ResourceTree.FromDirectory(directory));

        public static SinglePageBuilder FromDirectory(DirectoryInfo directory) => From(ResourceTree.FromDirectory(directory));

    }

}
