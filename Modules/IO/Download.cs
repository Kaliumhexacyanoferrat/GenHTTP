using System.IO;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Download
    {

        public static DownloadProviderBuilder FromString(string content) => From(Resource.FromString(content).Type(new FlexibleContentType(ContentType.TextPlain)));

        public static DownloadProviderBuilder FromFile(string file) => FromFile(new FileInfo(file));

        public static DownloadProviderBuilder FromFile(FileInfo file) => From(Resource.FromFile(file));

        public static DownloadProviderBuilder FromResource(string fileName) => FromResource(Assembly.GetCallingAssembly(), fileName);

        public static DownloadProviderBuilder FromResource(Assembly source, string fileName) => From(Resource.FromAssembly(source, fileName).Name(fileName));

        public static DownloadProviderBuilder From(IBuilder<IResource> resource) => From(resource.Build());

        public static DownloadProviderBuilder From(IResource resource) => new DownloadProviderBuilder().Resource(resource);

    }

}
