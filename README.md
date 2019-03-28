# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# with no dependencies to 3rd-party libraries. The main purpose of this project is to serve small web applications written in .NET, allowing developers to concentrate on the functionality rather than on the infrastructure.

As an example, the website of this project is hosted on a Raspberry Pi behind a nginx reverse proxy: [GenHTTP Website](https://genes.pics/genhttp/website/)

## Getting Started

To create a simple hello world project, run the following comand from the terminal:

```sh
dotnet new console --framework netcoreapp3.0 -o ExampleWebsite
```

This will create a new folder `ExampleWebsite`. Within this folder, run the following command to add a nuget package reference to the [GenHTTP Core package](https://www.nuget.org/packages/GenHTTP.Core/):

```sh
cd ExampleWebsite
dotnet add package GenHttp.Core
```

You can then edit the generated `Program.cs` to setup a simple project using the GenHTTP server API:

```csharp
var index = Page.From("Hello World!")
                .Title("Example Website");

var project = Layout.Create()
                    .Add("index", index, true);

var server = Server.Create()
                   .Router(project);

using (var instance = server.Build())
{
    Console.WriteLine("Press any key to shutdown ...");
    Console.ReadLine();
}
```

To run the newly created project, execute:

```sh
dotnet run 
```

This will host a new server instance on port 8080, allowing you to view the newly created project in your browser by navigating to http://localhost:8080.

## Building the Server

To build the server from source, clone this repository and run the example project launcher for .NET Core:

```sh
git clone https://github.com/Kaliumhexacyanoferrat/GenHTTP.git
cd ./GenHTTP/Examples/GenHTTP.Examples.CoreApp
dotnet run
```

This will build the example project launcher for .NET Core with all the server dependencies and launch the server process on port 8080. You can access the examples in the browser via http://localhost:8080.

## History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache failed to render dynamic pages on such a slow CPU back then. The original project description can still be found on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written in C#.

## Thanks

- [colorlib.](https://colorlib.com/) for the template of the sample project
- [scriban](https://github.com/lunet-io/scriban) for the templating engine
- [Raspberry Pi Foundation](https://www.raspberrypi.org/) and [.NET Core](https://github.com/dotnet/core) for a nice playground