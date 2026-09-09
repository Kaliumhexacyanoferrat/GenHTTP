using GenHTTP.Api.Content;

using GenHTTP.Modules.Files;

namespace GenHTTP.Playground.Samples;

public static class StaticFileSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Serves files from a given directory or resource tree.
         *
         * See https://genhttp.org/documentation/content/handlers/static-content/
         *
         */

        // e.g. GET http://localhost:8080/assets/GenHTTP.Playground.xml
        return Assets.From("./");
    }

}
