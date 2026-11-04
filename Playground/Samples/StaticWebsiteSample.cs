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

        // embedded into the assembly here, use ResourceTree.FromDirectory("/var/www") to serve a folder
        var files = ResourceTree.FromAssembly("Resources.Website");

        // GET http://localhost:8080/static-website/ serves index.html
        return StaticWebsite.From(files);
    }

}

