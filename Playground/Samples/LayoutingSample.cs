using GenHTTP.Api.Content;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples;

public static class LayoutingSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Layouts allow you to structure your web application and divide it into
         * different logical parts. The layout will route incoming requests to the designated
         * handler. Layouts can be nested as needed.
         *
         * See https://genhttp.org/documentation/content/handlers/layouting/
         * 
         */

        // serve static files from the current directory
        var assets = Assets.From("./");

        // define a simple REST API
        var api = Inline.Create()
                        .Get(() => "Hello World!");

        // assemble our application from the parts above and
        // register them at the given URLs
        return Layout.Create()
                     .Add("assets", assets) // e.g. GET http://localhost:8080/assets/GenHTTP.Playground.xml
                     .Add("api", api); // e.g. GET http://localhost:8080/api/
    }

}
