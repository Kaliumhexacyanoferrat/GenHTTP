# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# with a strong focus on developer experience. The main
purpose of this project is to quickly create web services written in .NET 8 / 9, allowing developers to concentrate on
the functionality rather than on messing around with configuration files or complex concepts.

[![CI](https://github.com/Kaliumhexacyanoferrat/GenHTTP/actions/workflows/ci.yml/badge.svg)](https://github.com/Kaliumhexacyanoferrat/GenHTTP/actions/workflows/ci.yml) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=GenHTTP&metric=coverage)](https://sonarcloud.io/dashboard?id=GenHTTP) [![nuget Package](https://img.shields.io/nuget/v/GenHTTP.Core.svg)](https://www.nuget.org/packages/GenHTTP.Core/) [](https://discord.gg/cW6tPJS7nt) [![Discord](https://discordapp.com/api/guilds/1177529388229734410/widget.png?style=shield)](https://discord.gg/GwtDyUpkpV)

## 🚀 Features

- Setup new webservices in a couple of minutes using [project templates](https://genhttp.org/documentation/content/templates/)
- Supports [current standards](https://genhttp.org/features/) such as Open API, Websockets, Server Sent Events or JWT authentication
- Embed web services into a new or already existing console, service, WPF, WinForms, WinUI, MAUI or Uno application
- Projects are fully described in code - no configuration files needed, no magical behavior you need to learn
- Optionally supports [Kestrel](https://genhttp.org/documentation/server/engines/) as an underlying HTTP engine (enables HTTP/2 and HTTP/3 via QUIC)
- [Optimized](https://genhttp.org/features/) out of the box, small memory and storage [footprint](https://genhttp.org/features/#footprint)
- Grade A+ security level according to SSL Labs

## 📖 Getting Started

This section shows how to create a new project from scratch using project templates and how to extend your existing
application by embedding the GenHTTP engine.

> [!NOTE]  
> This is a brief overview to get you running. You might want to have a look at
> the [tutorials](https://genhttp.org/documentation/tutorials/) for detailed step-by-step guides.

### New Project

Project templates can be used to create apps for typical use cases with little effort. After installing
the [.NET SDK](https://dotnet.microsoft.com/en-us/download) and the templates via `dotnet new -i GenHTTP.Templates` in
the terminal, the templates are available via the console or directly in Visual Studio:

<img src="https://user-images.githubusercontent.com/4992119/146939721-2970d28c-61bc-4a9a-b924-d483f97c8d8e.png" style="width: 30em;" />

To create a project by using the terminal, create a new folder for your app and use one of the following commands:

| Template                      | Command                                     | Documentation                                                                                                    |
|-------------------------------|---------------------------------------------|------------------------------------------------------------------------------------------------------------------|
| REST Webservice               | `dotnet new genhttp-webservice`             | [Webservices](https://genhttp.org/documentation/content/frameworks/webservices/)                                 |
| REST Webservice (single file) | `dotnet new genhttp-webservice-minimal`     | [Functional Handlers](https://genhttp.org/documentation/content/frameworks/functional/)                          |
| REST Webservice (controllers) | `dotnet new genhttp-webservice-controllers` | [Controllers](https://genhttp.org/documentation/content/frameworks/controllers/)                                 |
| Websocket                     | `dotnet new genhttp-websocket`              | [Websockets](https://genhttp.org/documentation/content/frameworks/websockets/)                                   |
| Server Sent Events (SSE)      | `dotnet new genhttp-sse`                    | [Server Sent Events](https://genhttp.org/documentation/content/handlers/server-sent-events/)                     |
| Website (Static HTML)         | `dotnet new genhttp-website-static`         | [Statics Websites](https://genhttp.org/documentation/content/frameworks/static-websites/)                        |
| Single Page Application (SPA) | `dotnet new genhttp-spa`                    | [Single Page Applications (SPA)](https://genhttp.org/documentation/content/frameworks/single-page-applications/) |

After the project has been created, you can run it via `dotnet run` and access the server via http://localhost:8080.

### Extending Existing Apps

If you would like to extend an existing .NET application, just add a nuget reference to the `GenHTTP.Core` nuget package. You can then spawn a new server instance with just a few lines of code:

```csharp
var content = Content.From(Resource.FromString("Hello World!"));

var host = await Host.Create()
                     .Handler(content)
                     .Defaults()
                     .StartAsync(); // or .RunAsync() to block until the application is shut down
```

When you run this sample it can be accessed in the browser via http://localhost:8080.

### Next Steps

The [documentation](https://genhttp.org/documentation/) provides a step-by-step starting guide as well as additional
information on how to
implement [webservices](https://genhttp.org/documentation/content/frameworks/webservices/), [minimal webservices](https://genhttp.org/documentation/content/frameworks/functional/), [controller-based webservices](https://genhttp.org/documentation/content/frameworks/controllers/), [static websites](https://genhttp.org/documentation/content/frameworks/static-websites/),
or [single page applications](https://genhttp.org/documentation/content/frameworks/single-page-applications/) and how
to [host your application](https://genhttp.org/documentation/hosting/) via Docker.

If you encounter issues implementing your application, feel free
to [join our Discord community](https://discord.gg/GwtDyUpkpV) to get help.

## ⚙️ Building the Server

To build the server from source, clone this repository and run the playground project launcher for .NET 9:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Playground
dotnet run
```

This will build the playground project launcher with all the server dependencies and launch the server process on port 8080. You can access the playground in the browser via http://localhost:8080.

## 🙌 Contributing

Writing a general purpose web application server is a tremendous task, so any contribution is very welcome. Besides
extending the server core, you might want to

- Leave a star on GitHub
- Extend the content capabilities of the server (e.g. by adding a new serialization format or rendering engine)
- Refine our [project templates](https://genhttp.org/documentation/content/templates/)
- Perform code reviews
- Analyze the performance or security of the server
- Clarfify and extend our tests
- Improve the documentation on the [website](https://genhttp.org/) or in code

If you would like to contribute, please also have a look at
the [contribution guidelines](https://github.com/Kaliumhexacyanoferrat/GenHTTP/blob/master/CONTRIBUTING.md) and
the [good first issues](https://github.com/Kaliumhexacyanoferrat/GenHTTP/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22).

## 🏺 History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache
failed to render dynamic pages on such a slow CPU back then. The original project description can still be found
on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source
code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written
in C#. In 2024 the focus has shifted towards API development, dropping support for generating graphical web applications.

## 📌 Links

- Related to
  GenHTTP: [Templates](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Templates) | [Website](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Website)
- Reference
  projects: [GenHTTP Gateway](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Gateway) | [MockH](https://github.com/Kaliumhexacyanoferrat/MockH)
- Similar
  projects: [EmbedIO](https://github.com/unosquare/embedio) | [NetCoreServer](https://github.com/chronoxor/NetCoreServer) | [Watson Webserver](https://github.com/jchristn/WatsonWebserver) | [SimpleW](https://github.com/stratdev3/SimpleW)

## 🙏 Thanks

- Powered by [.NET](https://github.com/dotnet/core)
- Less allocations thanks to [PooledAwait](https://github.com/mgravell/PooledAwait)
- Modules implemented with [NSwag](https://github.com/RicoSuter/NSwag) (Open API), [Fleck](https://github.com/statianzo/Fleck) (WebSockets)
