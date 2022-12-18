# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# with only a few dependencies to 3rd-party libraries. The main purpose of this project is to quickly create feature rich web applications and web services written in .NET 6/7, allowing developers to concentrate on the functionality rather than on messing around with configuration files, CSS or bundling JS files. Projects are mainly written in .NET, which allows C# developers to use their familiar toolset in web application development as well.

As an example, the website of this project is hosted on a Raspberry Pi: [genhttp.org](https://genhttp.org/)

[![CI](https://github.com/Kaliumhexacyanoferrat/GenHTTP/actions/workflows/ci.yml/badge.svg)](https://github.com/Kaliumhexacyanoferrat/GenHTTP/actions/workflows/ci.yml) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=GenHTTP&metric=coverage)](https://sonarcloud.io/dashboard?id=GenHTTP) [![nuget Package](https://img.shields.io/nuget/v/GenHTTP.Core.svg)](https://www.nuget.org/packages/GenHTTP.Core/)

## Features

- Setup new webservices or websites in a couple of minutes using [project templates](https://genhttp.org/documentation/content/templates)
- Embed web services and applications into your existing console, service, WPF or WinForms application
- Projects are fully described in code - no configuration files needed
- [Optimized](https://genhttp.org/features) out of the box (e.g. by bundling resources or compressing results)
- Small memory and storage [footprint](https://genhttp.org/features#footprint)
- Several [themes](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes) available to be chosen from
- Grade A+ security level according to SSL Labs
- Can be used to mock HTTP responses in component testing (see [MockH](https://github.com/Kaliumhexacyanoferrat/MockH))

## Getting Started

Project templates can be used to create apps for typical use cases with little effort. After installing the templates via `dotnet new -i GenHTTP.Templates` in the terminal, the templates are available via the console or directly in Visual Studio:

<img src="https://user-images.githubusercontent.com/4992119/146939721-2970d28c-61bc-4a9a-b924-d483f97c8d8e.png" style="width: 30em;" />

If you would like to extend an existing .NET application, just add a nuget reference to the `GenHTTP.Core` nuget package. You can then spawn a new server instance with just a few lines of code:

```csharp
var content = Content.From(Resource.FromString("Hello World!"));

using var server = Host.Create()
                       .Handler(content)
                       .Defaults()
                       .Start(); // or .Run() to block until the application is shut down
```

When you run this sample it can be accessed in the browser via http://localhost:8080. 

The [documentation](https://genhttp.org/documentation/) provides a step-by-step starting guide as well as additional information on how to implement [webservices](https://genhttp.org/documentation/content/webservices), [websites](https://genhttp.org/documentation/content/websites), [MVC style projects](https://genhttp.org/documentation/content/controllers), or [single page applications](https://genhttp.org/documentation/content/single-page-applications) and how to [host your application](https://genhttp.org/documentation/hosting/) via Docker.

## Building the Server

To build the server from source, clone this repository and run the playground project launcher for .NET 7:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Playground
dotnet run
```

This will build the playground project launcher with all the server dependencies and launch the server process on port 8080. You can access the playground in the browser via http://localhost:8080.

## Contributing

Writing a general purpose web application server is a tremendous task, so any contribution is very welcome. Besides extending the server core, you might want to

- Extend the content capabilities of the server (e.g. by adding a new serialization format or rendering engine)
- Add a new [theme](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes)
- Refine our [project templates](https://genhttp.org/documentation/content/templates)
- Perform code reviews
- Analyze the performance or security of the server
- Clarfify and extend our tests
- Improve the documentation on the [website](https://genhttp.org/) or in code

If you would like to contribute, please also have a look at the [contribution guidelines](https://github.com/Kaliumhexacyanoferrat/GenHTTP/blob/master/CONTRIBUTING.md) and the [good first issues](https://github.com/Kaliumhexacyanoferrat/GenHTTP/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22).

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache failed to render dynamic pages on such a slow CPU back then. The original project description can still be found on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written in C#.

## Links

- Related to GenHTTP: [Templates](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Templates) | [Themes](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes) | [Website](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Website)
- Reference projects: [GenHTTP Gateway](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Gateway) | [MockH](https://github.com/Kaliumhexacyanoferrat/MockH)
- Similar projects: [EmbedIO](https://github.com/unosquare/embedio) | [NetCoreServer](https://github.com/chronoxor/NetCoreServer) | [Watson Webserver](https://github.com/jchristn/WatsonWebserver)

## Thanks

- [.NET 7](https://github.com/dotnet/core) for a nice platform
