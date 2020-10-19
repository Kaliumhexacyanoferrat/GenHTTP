using System.IO;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Providers;
using GenHTTP.Api.Content.IO;

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
            return From(Resource.FromFile(file).Build()).Type(file.FullName.GuessContentType() ?? ContentType.ApplicationForceDownload);
        }

        public static DownloadProviderBuilder FromResource(string name)
        {
            return FromResource(Assembly.GetCallingAssembly(), name);
        }

        public static DownloadProviderBuilder FromResource(Assembly source, string name)
        {
            return From(Resource.FromAssembly(source, name).Build()).Type(name.GuessContentType() ?? ContentType.ApplicationForceDownload);
        }

        public static DownloadProviderBuilder From(IBuilder<IResource> resource)
        {
            return From(resource.Build());
        }

        public static DownloadProviderBuilder From(IResource resource)
        {
            return new DownloadProviderBuilder().Resource(resource)
                                                .Type(ContentType.ApplicationForceDownload);
        }

    }

}
