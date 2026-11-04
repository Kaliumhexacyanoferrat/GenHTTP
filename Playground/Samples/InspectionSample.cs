using GenHTTP.Api.Content;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Inspection;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples;

public static class InspectionSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Helps to find out why a client receives a certain response. Append "?inspect"
         * to any URL to see the request as the server received it, together with
         * the response the server would have sent.
         *
         * As this exposes details about your server, only add it while developing.
         *
         * See https://genhttp.org/documentation/content/concerns/inspection/
         *
         */

        var orders = Inline.Create()
                           .Get((int limit) => Enumerable.Range(1, limit).Select(i => new Order(i)));

        return Layout.Create()
                     .Add("api/orders", orders) // GET http://localhost:8080/inspection/api/orders/?limit=5&inspect
                     .AddInspector();
    }

    public record Order(int Id);

}
