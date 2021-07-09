using GenHTTP.Modules.IO;
using GenHTTP.Modules.Websites.Sites;

namespace GenHTTP.Modules.AutoReload
{

    public static class Extensions
    {

        public static WebsiteBuilder AutoReload(this WebsiteBuilder builder)
        {
            builder.AddScript("genhttp-autoreload.js", Resource.FromAssembly("AutoReload.js"), true);

            return builder;
        }

    }

}
