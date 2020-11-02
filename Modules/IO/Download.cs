using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Download
    {

        public static DownloadProviderBuilder From(IBuilder<IResource> resource) => From(resource.Build());

        public static DownloadProviderBuilder From(IResource resource) => new DownloadProviderBuilder().Resource(resource);

    }

}
