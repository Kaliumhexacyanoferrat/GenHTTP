using GenHTTP.Api.Content;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;

namespace GenHTTP.Playground.Samples;

public static class StaticWebsiteSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Serves a static website from a directory (or resource tree in general).
         *
         * See https://genhttp.org/documentation/content/frameworks/static-websites/
         *
         */

        var files = ResourceTree.FromDirectory("/var/www");

        return StaticWebsite.From(files);
    }

}

