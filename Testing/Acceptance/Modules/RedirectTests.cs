using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class RedirectTests
{

    [TestMethod]
    public async Task TestTemporary()
    {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestHost.Run(redirect);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

    [TestMethod]
    public async Task TestTemporaryPost()
    {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestHost.Run(redirect);

            var request = runner.GetRequest();
            request.Method = HttpMethod.Post;

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.SeeOther);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

    [TestMethod]
    public async Task TestPermanent()
    {
            var redirect = Redirect.To("https://google.de/");

            using var runner = TestHost.Run(redirect);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.MovedPermanently);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

    [TestMethod]
    public async Task TestPermanentPost()
    {
            var redirect = Redirect.To("https://google.de/", false);

            using var runner = TestHost.Run(redirect);

            var request = runner.GetRequest();
            request.Method = HttpMethod.Post;

            using var response = await runner.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.PermanentRedirect);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

    [TestMethod]
    public async Task TestAbsoluteRoute()
    {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("/me/to/"));

            using var runner = TestHost.Run(layout);

            using var response = await runner.GetResponseAsync("/redirect/");

            Assert.AreEqual("/me/to/", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

}
