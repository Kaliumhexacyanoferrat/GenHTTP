using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core
{

    public static class Download
    {

        public static DownloadProviderBuilder FromFile(string file)
        {
            return FromFile(new FileInfo(file));
        }

        public static DownloadProviderBuilder FromFile(FileInfo file)
        {
            return From(Data.FromFile(file).Build()).Type(file.FullName.GuessContentType());
        }

        public static DownloadProviderBuilder FromResource(string name)
        {
            return FromResource(Assembly.GetCallingAssembly(), name);
        }

        public static DownloadProviderBuilder FromResource(Assembly source, string name)
        {
            return From(Data.FromResource(source, name).Build()).Type(name.GuessContentType());
        }

        public static DownloadProviderBuilder From(IBuilder<IResourceProvider> resource)
        {
            return From(resource.Build());
        }

        public static DownloadProviderBuilder From(IResourceProvider resource)
        {
            return new DownloadProviderBuilder().Resource(resource)
                                                .Type(ContentType.ApplicationForceDownload);
        }

    }

}
