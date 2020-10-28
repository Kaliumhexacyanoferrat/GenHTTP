using System.IO;
using System.Reflection;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    
    public static class Resources
    {

        public static ResourceHandlerBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

        public static ResourceHandlerBuilder From(IResourceTree tree) => new ResourceHandlerBuilder().Tree(tree);

        public static ResourceHandlerBuilder FromAssembly(string root) => FromAssembly(Assembly.GetCallingAssembly(), root);

        public static ResourceHandlerBuilder FromAssembly(Assembly source, string root) => From(ResourceTree.FromAssembly(source, root));

        public static ResourceHandlerBuilder FromDirectory(string directory) => FromDirectory(new DirectoryInfo(directory));

        public static ResourceHandlerBuilder FromDirectory(DirectoryInfo directory) => From(ResourceTree.FromDirectory(directory));

    }

}
