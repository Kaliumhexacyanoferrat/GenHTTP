using System;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public class RedirectTests
    {

        [TestMethod]
        public void TestTemporary()
        {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestRunner.Run(redirect);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.AreEqual("https://google.de/", response.Headers["Location"]);
        }

        [TestMethod]
        public void TestTemporaryPost()
        {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestRunner.Run(redirect);

            var request = runner.GetRequest();
            request.Method = "POST";

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.AreEqual("https://google.de/", response.Headers["Location"]);
        }

        [TestMethod]
        public void TestPermanent()
        {
            var redirect = Redirect.To("https://google.de/");

            using var runner = TestRunner.Run(redirect);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.AreEqual("https://google.de/", response.Headers["Location"]);
        }

        [TestMethod]
        public void TestPermanentPost()
        {
            var redirect = Redirect.To("https://google.de/", false);

            using var runner = TestRunner.Run(redirect);

            var request = runner.GetRequest();
            request.Method = "POST";

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.PermanentRedirect, response.StatusCode);
            Assert.AreEqual("https://google.de/", response.Headers["Location"]);
        }

        [TestMethod]
        public void TestSimpleRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("{index}"))
                               .Index(Content.From(Resource.FromString("Hello World")));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect");

            Assert.AreEqual("/", new Uri(response.Headers["Location"]!).AbsolutePath);
        }

        [TestMethod]
        public void TestSimpleRelativeRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("./me/to"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect/");

            Assert.AreEqual("/redirect/me/to", new Uri(response.Headers["Location"]!).AbsolutePath);
        }

        [TestMethod]
        public void TestNavigatedRelativeRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("../me/to"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect/");

            Assert.AreEqual("/me/to", new Uri(response.Headers["Location"]!).AbsolutePath);
        }

        [TestMethod]
        public void TestAbsoluteRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("/me/to/"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect/");

            Assert.AreEqual("/me/to/", new Uri(response.Headers["Location"]!).AbsolutePath);
        }

    }

}
