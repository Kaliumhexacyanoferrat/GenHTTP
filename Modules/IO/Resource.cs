using System.IO;
using System.Reflection;

using GenHTTP.Modules.IO.Embedded;
using GenHTTP.Modules.IO.FileSystem;
using GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Modules.IO
{

    public static class Resource
    {

        public static StringResourceBuilder FromString(string data) => new StringResourceBuilder().Content(data);

        public static EmbeddedResourceBuilder FromAssembly(string name) => new EmbeddedResourceBuilder().Path(name).Assembly(Assembly.GetCallingAssembly());

        public static EmbeddedResourceBuilder FromAssembly(Assembly assembly, string name) => new EmbeddedResourceBuilder().Assembly(assembly).Path(name);

        public static FileResourceBuilder FromFile(string file) => FromFile(new FileInfo(file));

        public static FileResourceBuilder FromFile(FileInfo file) => new FileResourceBuilder().File(file);

    }

}
