using System.IO;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Download
    {

        public static DownloadProviderBuilder FromFile(string file)
        {
            return FromFile(new FileInfo(file));
        }

        public static DownloadProviderBuilder FromFile(FileInfo file)
        {
            return From(Data.FromFile(file).Build()).Type(file.FullName.GuessContentType() ?? ContentType.ApplicationForceDownload);
        }

        public static DownloadProviderBuilder FromResource(string name)
        {
            return FromResource(Assembly.GetCallingAssembly(), name);
        }

        public static DownloadProviderBuilder FromResource(Assembly source, string name)
        {
            return From(Data.FromResource(source, name).Build()).Type(name.GuessContentType() ?? ContentType.ApplicationForceDownload);
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
