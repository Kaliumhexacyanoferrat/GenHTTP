# GenHTTP Webserver

GenHTTP is a lightweight, modular web server written in pure C# with a strong focus on developer experience. The main
purpose of this project is to quickly create web services written in .NET 10 / 11, allowing developers to concentrate on
the functionality rather than on messing around with configuration files or complex concepts.

[![View - Documentation](https://img.shields.io/badge/view-Documentation-AB54FF)](https://genhttp.org/documentation/) [![nuget Package](https://img.shields.io/nuget/v/GenHTTP.Full.svg)](https://www.nuget.org/packages/GenHTTP.Full/) [![HTTP Arena](https://img.shields.io/endpoint?url=https://www.http-arena.com/badge/genhttp/h1.json)](https://www.http-arena.com/#type=emerging,flagship&tuned=0) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=GenHTTP&metric=coverage)](https://sonarcloud.io/dashboard?id=GenHTTP) [![Discord](https://discordapp.com/api/guilds/1177529388229734410/widget.png?style=shield)](https://discord.gg/PRkwKrnrB4)

## Getting Started

To host a GenHTTP server instance in an existing or new .NET project, add a reference to `GenHTTP.Full` to your
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

When running this snippet, the host will answer any request to http://localhost:8080 with a "Hello World" text response,
serve an Open API specification at http://localhost:8080/openapi.json and provide a graphical API
viewer on http://localhost:8080/scalar/.

## Samples

This section contains a few typical examples to get you started. See [the documentation](https://genhttp.org/documentation/content/)
for all available capabilities. Additional, runnable samples can be found in the [playground](./Playground/) project.

## Support

If you encounter issues implementing your application, feel free
to [join our Discord community](https://discord.gg/PRkwKrnrB4) to get help.

For commercial products and projects, GenHTTP provides additional support options
[on request](https://genhttp.org/support/).

## Platforms & Releases

GenHTTP targets all .NET versions currently [supported by Microsoft](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
Major versions are released once a year, following the .NET release cycle.
Additionally, our automated tests ensure full compatibility on the following platforms:

| OS      | Architectures           |
|---------|-------------------------|
| Linux   | `x64`, `arm32`, `arm64` |
| Windows | `x64`, `arm64`          |
| macOS   | `x64`, `arm64`          |

## Building the Server

To build the server from source, clone this repository and run the playground project launcher for .NET 11:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Playground
dotnet run
```

This will build the playground project launcher with all the server dependencies and launch the server process on port 8080. You can access the playground in the browser via http://localhost:8080.

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache
failed to render dynamic pages on such a slow CPU back then. The original project description can still be found
on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source
code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written
in C#. In 2024 the focus has shifted towards API development, dropping support for generating graphical web applications.

## Thanks

- Powered by [.NET](https://github.com/dotnet/core)
- Modules implemented with [NSwag](https://github.com/RicoSuter/NSwag) | [Cottle](https://r3c.github.io/cottle/) | [SharpCompress](https://github.com/adamhathcock/sharpcompress)

### Supported by

[![JetBrains logo.](https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg)](https://jb.gg/OpenSource) 
