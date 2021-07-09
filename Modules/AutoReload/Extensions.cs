using GenHTTP.Modules.IO;
using GenHTTP.Modules.Websites.Sites;

namespace GenHTTP.Modules.AutoReload
{

    public static class Extensions
    {

        /// <summary>
        /// Automatically instructs the browser to reload the current page
        /// if it somehow changed.
        /// </summary>
        /// <remarks>
        /// This method will add an additional script to your website which will
        /// poll the server for changes to the ETag header field. It is highly
        /// recommended to use this functionality in development mode only.
        /// </remarks>
        public static WebsiteBuilder AutoReload(this WebsiteBuilder builder)
        {
            builder.AddScript("genhttp-modules-autoreload.js", Resource.FromAssembly("AutoReload.js"), false);

            return builder;
        }

    }

}
