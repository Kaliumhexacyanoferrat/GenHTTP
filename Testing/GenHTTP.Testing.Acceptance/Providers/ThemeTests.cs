using System.Collections.Generic;
using System.Net;

using Xunit;

using GenHTTP.Api.Modules.Websites;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Websites;
using GenHTTP.Modules.Themes.Arcana;
using GenHTTP.Modules.Themes.Lorahost;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ThemeTests
    {

        [Theory]
        [MemberData(nameof(Themes))]
        public void TestBundles(ITheme theme)
        {
            using var runner = TestRunner.Run(GetWebsite(theme));

            using var script = runner.GetResponse("/scripts/bundle.js");
            Assert.Equal(HttpStatusCode.OK, script.StatusCode);

            using var style = runner.GetResponse("/styles/bundle.css");
            Assert.Equal(HttpStatusCode.OK, style.StatusCode);
        }

        [Theory]
        [MemberData(nameof(Themes))]
        public void TestIndex(ITheme theme)
        {
            using var runner = TestRunner.Run(GetWebsite(theme));

            using var index = runner.GetResponse();
            Assert.Equal(HttpStatusCode.OK, index.StatusCode);
        }

        [Theory]
        [MemberData(nameof(Themes))]
        public void TestErrorHandling(ITheme theme)
        {
            using var runner = TestRunner.Run(GetWebsite(theme));

            using var index = runner.GetResponse("/idonotexist");
            Assert.Equal(HttpStatusCode.NotFound, index.StatusCode);
        }

        private WebsiteBuilder GetWebsite(ITheme theme)
        {
            var layout = Layout.Create()
                               .Add("index", Page.From("Index"), true)
                               .Add("page1", Page.From("Page 1"))
                               .Add("page2", Page.From("Page 2"));

            return Website.Create()
                          .Theme(theme)
                          .Content(layout);
        }

        #region Theme registration & setup

        public static IEnumerable<object[]> Themes => new List<object[]>
        {
            new object[] { GetLorahost() },
            new object[] { GetArcana() }
        };

        private static ITheme GetLorahost()
        {
            return new LorahostBuilder().Copyright("2020 - UATs")
                                        .Title("Some Title")
                                        .Subtitle("Some Subtitle")
                                        .Action("Do something", "/someaction")
                                        .Header(Data.FromString("Broken Image :("))
                                        .Build();
        }

        private static ITheme GetArcana()
        {
            var footer = Menu.Empty()
                             .Add("page1", "Page 1")
                             .Add("page2", "Page 2", new List<(string, string)> { ("{root}", "Root"), ("{root}/page1", "Page 1") });

            return new ArcanaBuilder().Title("Some Title")
                                      .Copyright("Copyright")
                                      .Footer1("Footer #1", footer)
                                      .Footer2("Footer #2", footer)                                    
                                      .Build();
        }

        #endregion

    }

}
