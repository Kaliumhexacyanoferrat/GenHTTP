# GenHTTP Webserver

GenHTTP is a lightweight web server written in pure C# and without any dependencies to 3rd-party libraries. It provides an object-oriented model to provide content and can either be embedded into another .NET Core application or be executed as a standalone application.

# History

The web server was originally developed in 2008 to run on a netbook with an Intel Atom processor. Both IIS and Apache failed to render dynamic pages on such a slow CPU back then. The original project description can still be found on [archive.org](https://web.archive.org/web/20100706192130/http://gene.homeip.net/GenHTTPWebsite/). In 2019, the source code has been moved to GitHub with the goal to rework the project to be able to run dockerized web applications written in C#.