using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;

namespace GenHTTP.Playground.Samples;

public static class CorsSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Allows a frontend hosted on another origin (e.g. a React app served by
         * the Vite dev server on http://localhost:5173) to call this API from the browser.
         * Browsers will block requests from any other origin.
         *
         * See https://genhttp.org/documentation/content/concerns/cors/
         *
         */

        var todos = new List<string> { "Write the frontend" };

        var api = Inline.Create()
                        .Get(() => todos) // GET http://localhost:8080/cors/api/todos/
                        .Put((string todo) => todos.Add(todo)); // PUT http://localhost:8080/cors/api/todos/?todo=...

        var policy = CorsPolicy.Restrictive()
                               .Add("http://localhost:5173", [RequestMethod.Get, RequestMethod.Put], allowedHeaders: null, exposedHeaders: null, allowCredentials: false);

        return Layout.Create()
                     .Add("api/todos", api)
                     .Add(policy);
    }

}
