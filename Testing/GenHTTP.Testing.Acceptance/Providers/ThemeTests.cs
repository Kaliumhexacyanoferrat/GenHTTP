using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Modules.Websites;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Websites;
using GenHTTP.Modules.Themes.Arcana;
using GenHTTP.Modules.Themes.Lorahost;
using GenHTTP.Testing.Acceptance.Domain;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ThemeTests
    {

        public static IEnumerable<object[]> Themes => new List<object[]>
        {
            new object[] { new LorahostBuilder().Build() },
            new object[] { new ArcanaBuilder().Build() }
        };

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

    }

}
