using System.IO;
using System.Reflection;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Data
    {

        public static ResourceDataProviderBuilder FromResource(string name)
        {
            return new ResourceDataProviderBuilder().Name(name).Assembly(Assembly.GetCallingAssembly());
        }

        public static ResourceDataProviderBuilder FromResource(Assembly assembly, string name)
        {
            return new ResourceDataProviderBuilder().Assembly(assembly).Name(name);
        }

        public static FileDataProviderBuilder FromFile(string file)
        {
            return FromFile(new FileInfo(file));
        }

        public static FileDataProviderBuilder FromFile(FileInfo file)
        {
            return new FileDataProviderBuilder().File(file);
        }

        public static StringDataProviderBuilder FromString(string data)
        {
            return new StringDataProviderBuilder().Content(data);
        }

    }

}
