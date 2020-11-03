using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Placeholders.Providers;

namespace GenHTTP.Modules.Placeholders
{

    public static class Page
    {

        public static PageProviderBuilder From(string title, string content)
        {
            return From(content).Title(title);
        }

        public static PageProviderBuilder From(string content)
        {
            return From(Resource.FromString(content));
        }

        public static PageProviderBuilder From(IBuilder<IResource> content)
        {
            return From(content.Build());
        }

        public static PageProviderBuilder From(IResource content)
        {
            return new PageProviderBuilder().Content(content);
        }

    }

}
