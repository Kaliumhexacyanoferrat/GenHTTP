# GenHTTP Playground

The playground provides code samples for quick reference and allows developers to
quickly validate things.

```bash
cd Playground
dotnet run
```

Then open http://localhost:8080/ to browse the samples, read their code and call them
from your browser (or copy the equivalent `curl` command). All samples run on this one
server, each below its own path, e.g. `/webservices/`.

To add a sample, create a class in [Samples](./Samples) with a `Create()` method returning
the handler and register it with a few example requests in [Browser/Catalog.cs](./Browser/Catalog.cs).

## Frameworks

| Sample                                                               | Description                                                                      |
|----------------------------------------------------------------------|----------------------------------------------------------------------------------|
| [Webservices](./Samples/WebserviceSample.cs)                         | Implements a REST webservice via a handler class.                                |
| [Functional](./Samples/FunctionalSample.cs)                          | Uses delegates to implement a web service (similar to ASP.NET Core Minimal API). |
| [Controllers](./Samples/ControllerSample.cs)                         | Uses controllers to implement a web service (similar to ASP.NET Core MVC).       |
| [Websockets](./Samples/WebsocketSample.cs)                           | A chat room that pushes messages to all connected browsers.                      |
| [MCP Tools](./Samples/McpSample.cs)                                  | Provides tools to AI agents such as Claude via the Model Context Protocol.       |
| [Static Websites](./Samples/StaticWebsiteSample.cs)                  | Hosts a static website (with `index.html` support).                              |
| [Single Page Applications](./Samples/SinglePageApplicationSample.cs) | Hosts a SPA such as a React or Angular application.                              |
| [Custom Frameworks](./Samples/CustomFrameworkSample.cs)              | Builds your own framework on top of the reflection layer.                        |

## Handlers

| Sample                                                  | Description                                                                 |
|---------------------------------------------------------|-----------------------------------------------------------------------------|
| [Layouting](./Samples/LayoutingSample.cs)               | Allows an app to use multiple handlers by adding routing.                   |
| [Static Files](./Samples/StaticFileSample.cs)           | Serves static files from a directory.                                       |
| [API Documentation](./Samples/ApiDocumentationSample.cs) | Generates an Open API definition and serves Swagger UI, Redoc and Scalar.  |
| [Server Sent Events](./Samples/ServerSentEventsSample.cs) | A live dashboard that receives updates pushed by the server.              |
| [Pages](./Samples/PagesSample.cs)                       | Renders HTML pages from templates on the server.                            |
| [Virtual Hosting](./Samples/VirtualHostingSample.cs)    | Hosts multiple websites, one per domain, on a single server.                |
| [Load Balancing](./Samples/LoadBalancerSample.cs)       | Distributes requests over workers and mirrors, with a canary deployment.    |

## Concerns

| Sample                                                | Description                                                                   |
|-------------------------------------------------------|-------------------------------------------------------------------------------|
| [Authentication](./Samples/AuthenticationSample.cs)   | Secures an API with API keys, roles and per-user data.                        |
| [CORS](./Samples/CorsSample.cs)                       | Allows a frontend on another origin to call the API from the browser.         |
| [Error Handling](./Samples/ErrorHandlingSample.cs)    | JSON errors for the API, HTML error pages for the website, custom mapping.    |
| [Localization](./Samples/LocalizationSample.cs)       | Responds in the language preferred by the user.                               |
| [Inspection](./Samples/InspectionSample.cs)           | Shows what the server received and responded, to debug clients.              |

## Concepts

| Sample                                                          | Description                                                     |
|-----------------------------------------------------------------|-----------------------------------------------------------------|
| [Dependency Injection](./Samples/DependencyInjectionSample.cs)  | Lets the framework construct services and pass dependencies.    |

## Use Cases

These samples combine multiple modules the way an application typically would.

| Sample                                                 | Description                                                                                   |
|--------------------------------------------------------|-----------------------------------------------------------------------------------------------|
| [File Share](./Samples/FileShareSample.cs)             | Password protected directory listing with resumable downloads.                                |
| [Legacy Migration](./Samples/LegacyMigrationSample.cs) | Replaces a legacy app step by step using a reverse proxy, redirects and new endpoints.        |

The [Ioxide](./Samples/Ioxide/README.md) folder contains samples for the io_uring based engine.
