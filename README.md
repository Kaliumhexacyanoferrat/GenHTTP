# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# and with only a few dependencies to 3rd-party libraries. The main purpose of this project is to serve small web applications written in .NET, allowing developers to concentrate on the functionality rather than on the infrastructure.

As an example, the website of this project is hosted on a Raspberry Pi behind a nginx reverse proxy: [GenHTTP Example Project](https://genes.pics/genhttp/website/)

## Getting Started

Currently, version 2 of the server is in development, with no nuget packages or Docker images yet available. This section will be extended, as soon as version 2 becomes stable.

For now, you can clone the repository and open the solution file in the directory root with Visual Studio 2019 or Visual Studio Code to build the server and the sample projects. The example project is a standalone [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0) application so it can directly be built and started.

To run the example project with .NET Core:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Examples/GenHTTP.ExampleProject
dotnet run
```

The example project will host a server instance on port 8080, so it can be viewed in your browser via http://localhost:8080.

To build the project for ARM32, run:

```sh
dotnet publish -r linux-arm
```

The resulting files can be found in `bin\Debug\netcoreapp3.0\linux-arm\publish` and deployed to the target system. To mark the app executable and run it:

```sh
chmod 775 GenHTTP.ExampleProject
./GenHTTP.ExampleProject
```

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache failed to render dynamic pages on such a slow CPU back then. The original project description can still be found on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written in C#.

## Thanks

- [colorlib.](https://colorlib.com/) for the template of the sample project
- [scriban](https://github.com/lunet-io/scriban) for the templating engine
- [Raspberry Pi Foundation](https://www.raspberrypi.org/) and [.NET Core](https://github.com/dotnet/core) for a nice playground