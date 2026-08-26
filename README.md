# GenHTTP Webserver

GenHTTP is a lightweight, modular web server written in pure C# with a strong focus on developer experience. The main
purpose of this project is to quickly create web services written in .NET 10 / 11, allowing developers to concentrate on
the functionality rather than on messing around with configuration files or complex concepts.

[![View - Documentation](https://img.shields.io/badge/view-Documentation-AB54FF)](https://genhttp.org/documentation/) [![nuget Package](https://img.shields.io/nuget/v/GenHTTP.Full.svg)](https://www.nuget.org/packages/GenHTTP.Full/) [![HTTP Arena](https://img.shields.io/endpoint?url=https://www.http-arena.com/badge/genhttp/h1.json)](https://www.http-arena.com/#type=emerging,flagship&tuned=0) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=GenHTTP&metric=coverage)](https://sonarcloud.io/dashboard?id=GenHTTP) [![Discord](https://discordapp.com/api/guilds/1177529388229734410/widget.png?style=shield)](https://discord.gg/PRkwKrnrB4)

## Getting Started

To host a GenHTTP server instance in an existing or new .NET project, add a nuget reference to `GenHTTP.Full` to your
project and spin off a new host:

```csharp
using GenHTTP.Engine.Internal;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;

// use a handler of your choice (see the samples below)
var api = Layout.Create()
                .Add(Inline.Create().Get(() => "Hello World"))
                .AddOpenApi()
                .AddScalar();

var host = await Host.Create()
                     .Handler(api)
                     .Defaults()
                     .StartAsync(); // or .RunAsync() to block until the (console) application is shut down
```

Running this snippet will provide the following endpoints:

| Endpoint                           | Description                                                            |
|------------------------------------|------------------------------------------------------------------------|
| http://localhost:8080              | Serves the API, answering requests with a "Hello World" text response. |
| http://localhost:8080/openapi.json | Serves the automatically generated Open API specification of the API.  |
| http://localhost:8080/scalar/      | Servers a graphical viewer of the API, using Scalar.                   |

## Samples

The [playground](./Playground/) project provides a quick starting point to view sample code and find more complex apps
built with GenHTTP. See [the documentation](https://genhttp.org/documentation/content/)
for all available capabilities.

| Sample                                                                          | Description                                               |
|---------------------------------------------------------------------------------|-----------------------------------------------------------|
| [Layouting](./Playground/Samples/LayoutingSample.cs)                            | Allows an app to use multiple handlers by adding routing. |
| [Static Files](./Playground/Samples/StaticFileSample.cs)                        | Serves static files from a directory.                     |
| [Static Websites](./Playground/Samples/StaticWebsiteSample.cs)                  | Hosts a static website (with `index.html` support).       |
| [Single Page Applications](./Playground/Samples/SinglePageApplicationSample.cs) | Hosts a SPA such as a React or Angular application.       |

## Support

If you encounter issues implementing your application, feel free
to [join our Discord community](https://discord.gg/PRkwKrnrB4) to get help.

For commercial products and projects, GenHTTP provides additional support options
[on request](https://genhttp.org/support/).

## Building the Server

To build the server from source, clone this repository and run the playground project launcher for .NET 11:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Playground
dotnet run
```

This will build the playground project launcher with all the server dependencies and launch the server process on port

8080. You can access the playground in the browser via http://localhost:8080.

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache
failed to render dynamic pages on such a slow CPU back then. The original project description can still be found
on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source
code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written
in C#. In 2024 the focus has shifted towards API development, dropping support for generating graphical web
applications.

## Thanks

- Powered by [.NET](https://github.com/dotnet/core)
- Modules implemented
  with [NSwag](https://github.com/RicoSuter/NSwag) | [Cottle](https://r3c.github.io/cottle/) | [SharpCompress](https://github.com/adamhathcock/sharpcompress)

### Supported by

[![JetBrains logo.](https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg)](https://jb.gg/OpenSource) 
