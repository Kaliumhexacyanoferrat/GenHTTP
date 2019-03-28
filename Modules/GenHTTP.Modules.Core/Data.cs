using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using GenHTTP.Modules.Core.Resource;

namespace GenHTTP.Modules.Core
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
            return new FileDataProviderBuilder().File(file);
        }

        public static FileDataProviderBuilder FromFile(FileInfo file)
        {
            return new FileDataProviderBuilder().File(file);
        }

    }

}
