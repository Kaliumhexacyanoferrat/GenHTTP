# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# with few dependencies to 3rd-party libraries. The main purpose of this project is to serve small web applications and web services written in .NET, allowing developers to concentrate on the functionality rather than on handling the infrastructure.

As an example, the website of this project is hosted on a Raspberry Pi: [genhttp.org](https://genhttp.org/)

![CI](https://github.com/Kaliumhexacyanoferrat/GenHTTP/workflows/Build/badge.svg) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=GenHTTP&metric=coverage)](https://sonarcloud.io/dashboard?id=GenHTTP) [![nuget Package](https://img.shields.io/nuget/v/GenHTTP.Core.svg)](https://www.nuget.org/packages/GenHTTP.Core/)

## Features

- Setup new webservices or websites in a couple of minutes using [project templates](https://genhttp.org/documentation/content/templates)
- [Optimized](https://genhttp.org/features) out of the box (e.g. by bundling resources or compressing results)
- Small memory and storage [footprint](https://genhttp.org/features#footprint)
- Several [themes](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes) available to be chosen from 
- Grade A+ security level according to SSL Labs

## Getting Started

After you added a reference to the `GenHTTP.Core` nuget package, you can spawn a new server instance with just a few lines of code:

```csharp
var content = Content.From(Resource.FromString("Hello World!"));

Host.Create()
    .Console()
    .Defaults()
    .Handler(content)
    .Run();
```

When you run this sample it can be accessed in the browser via http://localhost:8080. The [documentation](https://genhttp.org/documentation/) provides a step-by-step starting guide as well as additional information on how to implement [webservices](https://genhttp.org/documentation/content/webservices), [websites](https://genhttp.org/documentation/content/websites), [MVC style projects](https://genhttp.org/documentation/content/controllers), or [single page applications](https://genhttp.org/documentation/content/single-page-applications) and how to [host your application](https://genhttp.org/documentation/hosting/) via Docker.

## Building the Server

To build the server from source, clone this repository and run the playground project launcher for .NET 5:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Playground
dotnet run
```

This will build the playground project launcher for .NET Core with all the server dependencies and launch the server process on port 8080. You can access the playground in the browser via http://localhost:8080.

If you would like to contribute, see the [contribution guidelines](https://github.com/Kaliumhexacyanoferrat/GenHTTP/blob/master/CONTRIBUTING.md).

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache failed to render dynamic pages on such a slow CPU back then. The original project description can still be found on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written in C#.

## Links

- Related to GenHTTP
  - [Templates](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Templates)
  - [Themes](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes)
  - [Website](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Website)
- Reference projects
  - [GenHTTP Gateway](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Gateway)
- Similar projects
  - [EmbedIO](https://github.com/unosquare/embedio)
  - [NetCoreServer](https://github.com/chronoxor/NetCoreServer)
  - [Watson Webserver](https://github.com/jchristn/WatsonWebserver)

## Thanks

- [.NET 5](https://github.com/dotnet/core) for a nice platform
