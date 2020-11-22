using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Robots.Provider;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class RobotsTests
    {

        [TestMethod]
        public void TestDefault()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default()));

            var result = GetRobots(runner);

            AssertX.Contains("User-agent: *", result);
            AssertX.Contains("Allow: /", result);

            AssertX.DoesNotContain("Sitemap", result);
        }

        [TestMethod]
        public void TestDirective()
        {
            var robots = BotInstructions.Empty()
                               .Directive(new string[] { "MyAgent 1", "MyAgent 2" },
                                          new string[] { "/allowed", "/alsoallowed" },
                                          new string[] { "/disallowed/", "/alsodisallowed" });

            using var runner = TestRunner.Run(GetTest(robots));

            var result = GetRobots(runner);

            AssertX.Contains("User-agent: MyAgent 1", result);
            AssertX.Contains("User-agent: MyAgent 2", result);

            AssertX.Contains("Allow: /allowed", result);
            AssertX.Contains("Allow: /alsoallowed", result);

            AssertX.Contains("Disallow: /disallowed/", result);
            AssertX.Contains("Disallow: /alsodisallowed", result);
        }

        [TestMethod]
        public void TestSitemap()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default().Sitemap()));

            var result = GetRobots(runner);

            AssertX.Contains("Sitemap: http://localhost/sitemap.xml", result);
        }

        [TestMethod]
        public void TestCustomSitemap()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default().Sitemap("sitemap.xml")));

            var result = GetRobots(runner);

            AssertX.Contains("Sitemap: http://localhost/sitemap.xml", result);
        }

        [TestMethod]
        public void TestAbsoluteSitemap()
        {
            using var runner = TestRunner.Run(GetTest(BotInstructions.Default().Sitemap("http://my/sitemap.xml")));

            var result = GetRobots(runner);

            AssertX.Contains("Sitemap: http://my/sitemap.xml", result);
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
