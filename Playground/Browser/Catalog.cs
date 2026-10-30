using GenHTTP.Playground.Samples;

namespace GenHTTP.Playground.Browser;

/// <summary>
/// All samples served by the playground, with the requests the
/// sample browser offers to try them.
/// </summary>
internal static class Catalog
{
    private const string Docs = "https://genhttp.org/documentation/content/";

    private static readonly Dictionary<string, string> Json = new() { ["Content-Type"] = "application/json" };

    internal static readonly List<Sample> Samples =
    [
        // Frameworks

        new("webservices", "Frameworks", "Webservices", "Implements a REST webservice via a handler class.",
            Docs + "frameworks/webservices/", "WebserviceSample.cs", WebserviceSample.Create(),
            Example.Request("List all books", "GET", "/webservices/books/"),
            Example.Request("Add a book", "PUT", "/webservices/books/", """{ "title": "The Hobbit" }""", Json),
            Example.Request("Delete a book", "DELETE", "/webservices/books/1")),

        new("functional", "Frameworks", "Functional", "Uses delegates to implement a web service (similar to ASP.NET Core Minimal API).",
            Docs + "frameworks/functional/", "FunctionalSample.cs", FunctionalSample.Create(),
            Example.Request("List all books", "GET", "/functional/books/"),
            Example.Request("Add a book", "PUT", "/functional/books/", """{ "title": "The Hobbit" }""", Json),
            Example.Request("Delete a book", "DELETE", "/functional/books/1")),

        new("controllers", "Frameworks", "Controllers", "Uses controllers to implement a web service (similar to ASP.NET Core MVC).",
            Docs + "frameworks/controllers/", "ControllerSample.cs", ControllerSample.Create(),
            Example.Request("List all books", "GET", "/controllers/books/"),
            Example.Request("Add a book", "PUT", "/controllers/books/create/", """{ "title": "The Hobbit" }""", Json),
            Example.Request("Delete a book", "DELETE", "/controllers/books/delete/1/")),

        new("websockets", "Frameworks", "Websockets", "A chat room that pushes messages to all connected browsers.",
            Docs + "frameworks/websockets/", "WebsocketSample.cs", WebsocketSample.Create(),
            Example.Page("Open the chat", "/websockets/", "Open it twice to chat with yourself.")),

        new("mcp", "Frameworks", "MCP Tools", "Provides tools to AI agents such as Claude via the Model Context Protocol.",
            Docs + "frameworks/mcp/", "McpSample.cs", McpSample.Create(),
            Example.Shell("Add the server to Claude Code", "claude mcp add --transport http library {origin}/mcp",
                          "Then ask Claude \"Which books about Rome are in the library?\""),
            Example.Request("List the tools, as an agent would", "POST", "/mcp/", """{ "jsonrpc": "2.0", "id": 1, "method": "tools/list" }""", Json),
            Example.Request("Call a tool", "POST", "/mcp/", """{ "jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": { "name": "find_books", "arguments": { "query": "Rome" } } }""", Json)),

        new("static-website", "Frameworks", "Static Websites", "Hosts a static website (with index.html support).",
            Docs + "frameworks/static-websites/", "StaticWebsiteSample.cs", StaticWebsiteSample.Create(),
            Example.Page("Open the website", "/static-website/")),

        new("spa", "Frameworks", "Single Page Applications", "Hosts a SPA such as a React or Angular application.",
            Docs + "frameworks/single-page-applications/", "SinglePageApplicationSample.cs", SinglePageApplicationSample.Create(),
            Example.Page("Open the application", "/spa/"),
            Example.Page("Open a client side route directly", "/spa/books/42", "The server delivers the application, which then renders the route.")),

        new("custom-framework", "Frameworks", "Custom Frameworks", "Builds your own framework on top of the reflection layer.",
            Docs + "frameworks/custom/", "CustomFrameworkSample.cs", CustomFrameworkSample.Create(),
            Example.Request("Call the exposed method", "GET", "/custom-framework/42"),
            Example.Page("Browse the generated API", "/custom-framework/scalar/")),

        // Handlers

        new("layouting", "Handlers", "Layouting", "Allows an app to use multiple handlers by adding routing.",
            Docs + "handlers/layouting/", "LayoutingSample.cs", LayoutingSample.Create(),
            Example.Request("Call the API", "GET", "/layouting/api/"),
            Example.Request("Fetch a file", "GET", "/layouting/assets/README.md")),

        new("static-files", "Handlers", "Static Files", "Serves static files from a directory.",
            Docs + "handlers/files/", "StaticFileSample.cs", StaticFileSample.Create(),
            Example.Request("Fetch a file", "GET", "/static-files/README.md")),

        new("api-docs", "Handlers", "API Documentation", "Generates an Open API definition and serves Swagger UI, Redoc and Scalar.",
            Docs + "handlers/api-browsing/", "ApiDocumentationSample.cs", ApiDocumentationSample.Create(),
            Example.Page("Swagger UI", "/api-docs/swagger/"),
            Example.Page("Redoc", "/api-docs/redoc/"),
            Example.Page("Scalar", "/api-docs/scalar/"),
            Example.Request("Fetch the Open API definition", "GET", "/api-docs/openapi.json"),
            Example.Request("Fetch a book that does not exist", "GET", "/api-docs/books/42", note: "404, as documented in the definition")),

        new("sse", "Handlers", "Server Sent Events", "A live dashboard that receives updates pushed by the server.",
            Docs + "handlers/server-sent-events/", "ServerSentEventsSample.cs", ServerSentEventsSample.Create(),
            Example.Page("Open the dashboard", "/sse/"),
            Example.Shell("Watch the raw event stream", "curl -N {origin}/sse/stats/")),

        new("pages", "Handlers", "Pages", "Renders HTML pages from templates on the server.",
            Docs + "handlers/pages/", "PagesSample.cs", PagesSample.Create(),
            Example.Page("Page rendered from a template", "/pages/books/"),
            Example.Page("Page in the server style", "/pages/about/")),

        new("virtual-hosting", "Handlers", "Virtual Hosting", "Hosts multiple websites, one per domain, on a single server.",
            Docs + "handlers/virtual-hosts/", "VirtualHostingSample.cs", VirtualHostingSample.Create(),
            Example.Page("Open the blog", "http://blog.localhost:8080/virtual-hosting/"),
            Example.Page("Open the shop", "http://shop.localhost:8080/virtual-hosting/"),
            Example.Page("Open with any other host", "/virtual-hosting/", "Redirects to the blog."),
            Example.Shell("Choose the site via the Host header", "curl -H \"Host: shop.localhost\" {origin}/virtual-hosting/")),

        new("load-balancing", "Handlers", "Load Balancing", "Distributes requests over workers and mirrors, with a canary deployment.",
            Docs + "handlers/load-balancer/", "LoadBalancerSample.cs", LoadBalancerSample.Create(),
            Example.Request("Call a worker", "GET", "/load-balancing/api/", note: "502, unless workers run on ports 5001 and 5002"),
            Example.Request("Call the canary worker", "GET", "/load-balancing/api/", headers: new() { ["X-Canary"] = "1" }, note: "502, unless a worker runs on port 5003"),
            Example.Shell("Download from a mirror", "curl -i {origin}/load-balancing/downloads/setup.exe", "Redirects to one of the mirrors.")),

        // Concerns

        new("authentication", "Concerns", "Authentication", "Secures an API with API keys, roles and per-user data.",
            Docs + "concerns/authentication/", "AuthenticationSample.cs", AuthenticationSample.Create(),
            Example.Request("Add a note as Alice", "PUT", "/authentication/notes/", "Buy milk", new() { ["X-API-Key"] = "alice-key", ["Content-Type"] = "text/plain" }),
            Example.Request("List the notes of Alice", "GET", "/authentication/notes/", headers: new() { ["X-API-Key"] = "alice-key" }),
            Example.Request("List all notes as Alice", "GET", "/authentication/notes/all", headers: new() { ["X-API-Key"] = "alice-key" }, note: "403, Alice is no administrator"),
            Example.Request("List all notes as Bob", "GET", "/authentication/notes/all", headers: new() { ["X-API-Key"] = "bob-key" }),
            Example.Request("Call without a key", "GET", "/authentication/notes/", note: "401")),

        new("cors", "Concerns", "CORS", "Allows a frontend on another origin to call the API from the browser.",
            Docs + "concerns/cors/", "CorsSample.cs", CorsSample.Create(),
            Example.Request("List the todos", "GET", "/cors/api/todos/"),
            Example.Shell("Send the preflight request of the frontend",
                          "curl -i -X OPTIONS -H \"Origin: http://localhost:5173\" -H \"Access-Control-Request-Method: PUT\" {origin}/cors/api/todos/",
                          "Browsers send this before the actual request. Try another origin to see the CORS headers disappear.")),

        new("errors", "Concerns", "Error Handling", "JSON errors for the API, HTML error pages for the website, custom mapping.",
            Docs + "concerns/error-handling/", "ErrorHandlingSample.cs", ErrorHandlingSample.Create(),
            Example.Request("Fetch an order", "GET", "/errors/api/orders/1"),
            Example.Request("Fetch an order that does not exist", "GET", "/errors/api/orders/42", note: "404 as JSON"),
            Example.Request("Order something that is sold out", "POST", "/errors/api/orders/", """{ "product": "Tea" }""", Json, "409, mapped from a custom exception"),
            Example.Page("Open a page that does not exist", "/errors/does-not-exist", "404 as HTML page")),

        new("localization", "Concerns", "Localization", "Responds in the language preferred by the user.",
            Docs + "concerns/localization/", "LocalizationSample.cs", LocalizationSample.Create(),
            Example.Request("Prefer Swiss German", "GET", "/localization/greeting/", headers: new() { ["Accept-Language"] = "de-CH, en;q=0.8" }),
            Example.Request("Choose French", "GET", "/localization/greeting/?lang=fr"),
            Example.Request("Prefer Japanese", "GET", "/localization/greeting/", headers: new() { ["Accept-Language"] = "ja" }, note: "Not supported, falls back to English")),

        new("inspection", "Concerns", "Inspection", "Shows what the server received and responded, to debug clients.",
            Docs + "concerns/inspection/", "InspectionSample.cs", InspectionSample.Create(),
            Example.Request("Call the API", "GET", "/inspection/api/orders/?limit=5"),
            Example.Request("Inspect the call", "GET", "/inspection/api/orders/?limit=5&inspect")),

        // Concepts

        new("dependency-injection", "Concepts", "Dependency Injection", "Lets the framework construct services and pass dependencies.",
            Docs + "concepts/dependency-injection/", "DependencyInjectionSample.cs", DependencyInjectionSample.Create(),
            Example.Request("List all books", "GET", "/dependency-injection/books/"),
            Example.Request("Add a book", "PUT", "/dependency-injection/books/", """{ "title": "Dune" }""", Json)),

        // Use cases

        new("file-share", "Use Cases", "File Share", "Password protected directory listing with resumable downloads.",
            Docs + "handlers/listing/", "FileShareSample.cs", FileShareSample.Create(),
            Example.Page("Browse the files", "/file-share/", "Log in as \"friend\" with password \"secret\"."),
            Example.Request("Download the first 100 bytes of a file", "GET", "/file-share/README.md",
                            headers: new() { ["Authorization"] = "Basic ZnJpZW5kOnNlY3JldA==", ["Range"] = "bytes=0-99" }, note: "206 Partial Content")),

        new("legacy-migration", "Use Cases", "Legacy Migration", "Replaces a legacy app step by step using a reverse proxy, redirects and new endpoints.",
            Docs + "handlers/reverse-proxy/", "LegacyMigrationSample.cs", LegacyMigrationSample.Create(),
            Example.Request("Call the rewritten API", "GET", "/legacy-migration/api/v2/status"),
            Example.Request("Open a page that has moved", "GET", "/legacy-migration/contact.php", note: "301 to the rewritten page"),
            Example.Request("Open a page of the legacy app", "GET", "/legacy-migration/login.php", note: "502, unless the legacy app runs on port 5000"))
    ];

}
