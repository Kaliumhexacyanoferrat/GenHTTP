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

        var files = ResourceTree.FromDirectory("/var/app");

        return SinglePageApplication.From(files);
    }

}

