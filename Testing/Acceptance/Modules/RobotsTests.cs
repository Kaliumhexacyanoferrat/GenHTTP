using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Robots.Provider;
using GenHTTP.Modules.Sitemaps;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class RobotsTests
    {

        [TestMethod]
        public async Task TestDefault()
        {
            using var runner = TestHost.Run(GetTest(BotInstructions.Default()));

            var result = await GetRobots(runner);

            AssertX.Contains("User-agent: *", result);
            AssertX.Contains("Allow: /", result);

            AssertX.DoesNotContain("Sitemap", result);
        }

        [TestMethod]
        public async Task TestDirective()
        {
            var robots = BotInstructions.Empty()
                               .Directive(new string[] { "MyAgent 1", "MyAgent 2" },
                                          new string[] { "/allowed", "/alsoallowed" },
                                          new string[] { "/disallowed/", "/alsodisallowed" });

            using var runner = TestHost.Run(GetTest(robots));

            var result = await GetRobots(runner);

            AssertX.Contains("User-agent: MyAgent 1", result);
            AssertX.Contains("User-agent: MyAgent 2", result);

            AssertX.Contains("Allow: /allowed", result);
            AssertX.Contains("Allow: /alsoallowed", result);

            AssertX.Contains("Disallow: /disallowed/", result);
            AssertX.Contains("Disallow: /alsodisallowed", result);
        }

        [TestMethod]
        public async Task TestSitemap()
        {
            using var runner = TestHost.Run(GetTest(BotInstructions.Default().Sitemap()));

            var result = await GetRobots(runner);

            AssertX.Contains("Sitemap: http://localhost/" + Sitemap.FILE_NAME, result);
        }

        [TestMethod]
        public async Task TestCustomSitemap()
        {
            using var runner = TestHost.Run(GetTest(BotInstructions.Default().Sitemap(Sitemap.FILE_NAME)));

            var result = await GetRobots(runner);

            AssertX.Contains("Sitemap: http://localhost/" + Sitemap.FILE_NAME, result);
        }

        [TestMethod]
        public async Task TestAbsoluteSitemap()
        {
            using var runner = TestHost.Run(GetTest(BotInstructions.Default().Sitemap("http://my/" + Sitemap.FILE_NAME)));

            var result = await GetRobots(runner);

            AssertX.Contains("Sitemap: http://my/" + Sitemap.FILE_NAME, result);
        }

        private static async Task<string> GetRobots(TestHost runner)
        {
            using var response = await runner.GetResponseAsync("/" + BotInstructions.FILE_NAME);

            return (await response.GetContentAsync()).Replace($":{runner.Port}", string.Empty);
        }

        private static IHandlerBuilder GetTest(RobotsProviderBuilder robots)
        {
            return Layout.Create()
                         .Add(BotInstructions.FILE_NAME, robots);
        }

    }

}
