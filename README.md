# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# with few dependencies to 3rd-party libraries. The main purpose of this project is to serve small web applications written in .NET, allowing developers to concentrate on the functionality rather than on the infrastructure.

As an example, the website of this project is hosted on a Raspberry Pi behind a GenHTTP-based [reverse proxy](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Gateway): [genhttp.org](https://genhttp.org/)

![CI](https://github.com/Kaliumhexacyanoferrat/GenHTTP/workflows/Build/badge.svg) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=GenHTTP&metric=coverage)](https://sonarcloud.io/dashboard?id=GenHTTP) [![nuget Package](https://img.shields.io/nuget/v/GenHTTP.Core.svg)](https://www.nuget.org/packages/GenHTTP.Core/) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FKaliumhexacyanoferrat%2FGenHTTP.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FKaliumhexacyanoferrat%2FGenHTTP?ref=badge_shield)

## Getting Started

To create a simple hello world project, follow the official <a href="https://genhttp.org/documentation/">starting guide</a>.

## Building the Server

To build the server from source, clone this repository and run the playground project launcher for .NET Core:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Playground
dotnet run
```

This will build the playground project launcher for .NET Core with all the server dependencies and launch the server process on port 8080. You can access the playground in the browser via http://localhost:8080.

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache failed to render dynamic pages on such a slow CPU back then. The original project description can still be found on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written in C#.

## Related Projects

- [GenHTTP.Website](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Website)
- [GenHTTP.Themes](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes)
- [GenHTTP.Gateway](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Gateway)

## Thanks

- [.NET Core](https://github.com/dotnet/core) for a nice platform
