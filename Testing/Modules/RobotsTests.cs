using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Robots.Provider;
using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class RobotsTests
    {

        [Fact]
        public void TestDefault()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default()));

            var result = GetRobots(runner);

            Assert.Contains("User-agent: *", result);
            Assert.Contains("Allow: /", result);

            Assert.DoesNotContain("Sitemap", result);
        }

        [Fact]
        public void TestDirective()
        {
            var robots = BotInstructions.Empty()
                               .Directive(new string[] { "MyAgent 1", "MyAgent 2" },
                                          new string[] { "/allowed", "/alsoallowed" },
                                          new string[] { "/disallowed/", "/alsodisallowed" });

            using var runner = TestRunner.Run(GetTest(robots));

            var result = GetRobots(runner);

            Assert.Contains("User-agent: MyAgent 1", result);
            Assert.Contains("User-agent: MyAgent 2", result);

            Assert.Contains("Allow: /allowed", result);
            Assert.Contains("Allow: /alsoallowed", result);

            Assert.Contains("Disallow: /disallowed/", result);
            Assert.Contains("Disallow: /alsodisallowed", result);
        }

        [Fact]
        public void TestSitemap()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default().Sitemap()));

            var result = GetRobots(runner);

            Assert.Contains("Sitemap: http://localhost/sitemap.xml", result);
        }

        [Fact]
        public void TestCustomSitemap()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default().Sitemap("sitemap.xml")));

            var result = GetRobots(runner);

            Assert.Contains("Sitemap: http://localhost/sitemap.xml", result);
        }

        [Fact]
        public void TestAbsoluteSitemap()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default().Sitemap("http://my/sitemap.xml")));

            var result = GetRobots(runner);

            Assert.Contains("Sitemap: http://my/sitemap.xml", result);
        }

        private string GetRobots(TestRunner runner)
        {
            using var response = runner.GetResponse("/robots.txt");

            return response.GetContent().Replace($":{runner.Port}", string.Empty);
        }

        private IHandlerBuilder GetTest(RobotsProviderBuilder robots)
        {
            return Layout.Create()
                         .Add("robots.txt", robots);
        }

    }

}
