using GenHTTP.Engine;
using GenHTTP.Modules.AutoLayout;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Websites;
using GenHTTP.Themes.Lorahost;
using System.Collections.Generic;

var layout = TreeLayout.From(ResourceTree.FromDirectory(@"C:\Work\GenHTTP\GenHTTP.Website\Project\Pages"));

var theme = Theme.Create()
                             .Header(Resource.FromAssembly("Header.jpg"))
                             .Title("GenHTTP Webserver")
                             .Subtitle("Simple and lightweight, embeddable HTTP webserver written in pure C# with few dependencies to 3rd-party libraries. Compatible with .NET 6/7/8.")
                             .Action("documentation/", "Get started");

var menu = Menu.Empty()
                           .Add("{website}", "Home")
                           .Add("features", "Features")
                           .Add("documentation/", "Documentation", new List<(string, string)> { ("content/", "Providing Content"), ("testing/", "Testing Apps"), ("server/", "Server Setup"), ("hosting/", "Hosting Apps"), ("asp-net-comparison", "Comparison with ASP.NET") })
                           .Add("links", "Links")
                           .Add("https://discord.gg/GwtDyUpkpV", "Discord")
                           .Add("https://github.com/Kaliumhexacyanoferrat/GenHTTP", "GitHub")
                           .Add("legal", "Legal");


var website = Website.Create()
                     .Content(layout)
                     .Theme(theme)
                     .Menu(menu);

Host.Create()
    .Handler(website)
    .Defaults()
    .Development()
    .Console()
    .Run();
