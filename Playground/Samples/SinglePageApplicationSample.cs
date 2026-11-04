using GenHTTP.Api.Content;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.SinglePageApplications;

namespace GenHTTP.Playground.Samples;

public static class SinglePageApplicationSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Serves a single page application (SPA) from a directory (or resource tree in general).
         *
         * See https://genhttp.org/documentation/content/frameworks/single-page-applications/
         *
         */

        // embedded into the assembly here, use ResourceTree.FromDirectory("/var/app") to serve your build output
        var files = ResourceTree.FromAssembly("Resources.Spa");

        // GET http://localhost:8080/spa/books/42 serves index.html, the route is rendered in the browser
        return SinglePageApplication.From(files)
                                    .ServerSideRouting();
    }

}

