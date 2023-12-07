using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class RedirectTests
    {

        [TestMethod]
        public async Task TestTemporary()
        {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestRunner.Run(redirect);

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestTemporaryPost()
        {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestRunner.Run(redirect);

            var request = runner.GetRequest();
            request.Method = HttpMethod.Post;

            using var response = await runner.GetResponse(request);

            await response.AssertStatusAsync(HttpStatusCode.SeeOther);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestPermanent()
        {
            var redirect = Redirect.To("https://google.de/");

            using var runner = TestRunner.Run(redirect);

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.MovedPermanently);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestPermanentPost()
        {
            var redirect = Redirect.To("https://google.de/", false);

            using var runner = TestRunner.Run(redirect);

            var request = runner.GetRequest();
            request.Method = HttpMethod.Post;

            using var response = await runner.GetResponse(request);

            await response.AssertStatusAsync(HttpStatusCode.PermanentRedirect);
            Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestSimpleRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("{index}"))
                               .Index(Content.From(Resource.FromString("Hello World")));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/redirect");

            Assert.AreEqual("/", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        [TestMethod]
        public async Task TestSimpleRelativeRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("./me/to"));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/redirect/");

            Assert.AreEqual("/redirect/me/to", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        [TestMethod]
        public async Task TestNavigatedRelativeRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("../me/to"));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/redirect/");

            Assert.AreEqual("/me/to", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        [TestMethod]
        public async Task TestAbsoluteRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("/me/to/"));

            using var runner = TestRunner.Run(layout);

            using var response = await runner.GetResponse("/redirect/");

            Assert.AreEqual("/me/to/", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

    }

}
