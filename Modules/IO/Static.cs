using System.IO;
using System.Reflection;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Static
    {

        public static EmbeddedResourcesProviderBuilder Resources(string root)
        {
            return Resources(Assembly.GetCallingAssembly(), root);
        }

        public static EmbeddedResourcesProviderBuilder Resources(Assembly source, string root)
        {
            return new EmbeddedResourcesProviderBuilder().Assembly(source).Root(root);
        }

        public static FileResourcesProviderBuilder Files(string directory)
        {
            return Files(new DirectoryInfo(directory));
        }

        public static FileResourcesProviderBuilder Files(DirectoryInfo directory)
        {
            return new FileResourcesProviderBuilder().Directory(directory);
        }

    }

}
