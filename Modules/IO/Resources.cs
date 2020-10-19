using System.IO;
using System.Reflection;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Resources
    {

        public static EmbeddedResourcesProviderBuilder FromAssembly(string root)
        {
            return FromAssembly(Assembly.GetCallingAssembly(), root);
        }

        public static EmbeddedResourcesProviderBuilder FromAssembly(Assembly source, string root)
        {
            return new EmbeddedResourcesProviderBuilder().Assembly(source).Root(root);
        }

        public static FileResourcesProviderBuilder FromDirectory(string directory)
        {
            return FromDirectory(new DirectoryInfo(directory));
        }

        public static FileResourcesProviderBuilder FromDirectory(DirectoryInfo directory)
        {
            return new FileResourcesProviderBuilder().Directory(directory);
        }

    }

}
