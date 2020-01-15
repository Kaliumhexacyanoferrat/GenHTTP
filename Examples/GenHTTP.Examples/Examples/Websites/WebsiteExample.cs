using System.Collections.Generic;

using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Themes.Arcana;

namespace GenHTTP.Examples.Examples.Websites
{

    public static class WebsiteExample
    {

        public static IRouterBuilder Create()
        {
            var documentation = Layout.Create()
                                      .Add("gettingstarted", Page.From("Getting Started", "Getting started"))
                                      .Add("content", Page.From("Content", "Content"))
                                      .Add("server", Page.From("Server", "Server"))
                                      .Add("hosting", Page.From("Hosting", "Hosting"))
                                      .Index("gettingstarted");

            var content = Layout.Create()
                                .Add("home", Page.From("Home", "Home"))
                                .Add("documentation", documentation)
                                .Add("links", Page.From("Links", "Links"))
                                .Add("legal", Page.From("Legal", "Legal"))
                                .Index("home");

            var menu = Menu.Empty().Add("home", "Home")
                                   .Add("documentation/", "Documentation", new List<(string, string)> { ("content", "Content"), ("server", "Server"), ("hosting", "Hosting") })
                                   .Add("links", "Links")
                                   .Add("https://github.com/Kaliumhexacyanoferrat/GenHTTP", "Source")
                                   .Add("legal", "Legal");

            var theme = Theme.Create()
                             .Title("Example Website")
                             .Copyright("© Genesis Technologies")
                             .Footer1("Footer 1", Menu.From(documentation.Build()))
                             .Footer2("Footer 2", Menu.From(content.Build()));

            return Website.Create()
                          .Theme(theme)
                          .Menu(menu)
                          .Content(content);
        }

    }

}
