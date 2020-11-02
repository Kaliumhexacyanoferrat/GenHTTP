using System.IO;
using System.Reflection;

using GenHTTP.Modules.IO.Embedded;
using GenHTTP.Modules.IO.FileSystem;

namespace GenHTTP.Modules.IO
{

    public static class ResourceTree
    {

        public static EmbeddedResourceTreeBuilder FromAssembly()
        {
            var assembly = Assembly.GetCallingAssembly();

            return FromAssembly(assembly, assembly.GetName().Name);
        }

        public static EmbeddedResourceTreeBuilder FromAssembly(string root) => FromAssembly(Assembly.GetCallingAssembly(), root);

        public static EmbeddedResourceTreeBuilder FromAssembly(Assembly source, string root) => new EmbeddedResourceTreeBuilder().Source(source).Root(root);

        public static DirectoryTreeBuilder FromDirectory(string directory) => FromDirectory(new DirectoryInfo(directory));

        public static DirectoryTreeBuilder FromDirectory(DirectoryInfo directory) => new DirectoryTreeBuilder().Directory(directory);

    }

}
