using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Content
    {

        public static ContentProviderBuilder From(IBuilder<IResource> resource) => From(resource.Build());

        public static ContentProviderBuilder From(IResource resource) => new ContentProviderBuilder().Resource(resource);

    }

}
