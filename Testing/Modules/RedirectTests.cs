using System;
using System.Net;

using Xunit;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class RedirectTests
    {

        [Fact]
        public void TestTemporary()
        {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestRunner.Run(redirect);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.Equal("https://google.de/", response.Headers["Location"]);
        }

        [Fact]
        public void TestTemporaryPost()
        {
            var redirect = Redirect.To("https://google.de/", true);

            using var runner = TestRunner.Run(redirect);

            var request = runner.GetRequest();
            request.Method = "POST";

            using var response = runner.GetResponse(request);

            Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal("https://google.de/", response.Headers["Location"]);
        }

        [Fact]
        public void TestPermanent()
        {
            var redirect = Redirect.To("https://google.de/");

            using var runner = TestRunner.Run(redirect);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("https://google.de/", response.Headers["Location"]);
        }

        [Fact]
        public void TestPermanentPost()
        {
            var redirect = Redirect.To("https://google.de/", false);

            using var runner = TestRunner.Run(redirect);

            var request = runner.GetRequest();
            request.Method = "POST";

            using var response = runner.GetResponse(request);

            Assert.Equal(HttpStatusCode.PermanentRedirect, response.StatusCode);
            Assert.Equal("https://google.de/", response.Headers["Location"]);
        }

        [Fact]
        public void TestSimpleRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("{index}"))
                               .Index(Content.From("Hello World"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect");

            Assert.Equal("/", new Uri(response.Headers["Location"]).AbsolutePath);
        }

        [Fact]
        public void TestSimpleRelativeRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("./me/to"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect/");

            Assert.Equal("/redirect/me/to", new Uri(response.Headers["Location"]).AbsolutePath);
        }

        [Fact]
        public void TestNavigatedRelativeRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("../me/to"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect/");

            Assert.Equal("/me/to", new Uri(response.Headers["Location"]).AbsolutePath);
        }

        [Fact]
        public void TestAbsoluteRoute()
        {
            var layout = Layout.Create()
                               .Add("redirect", Redirect.To("/me/to/"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/redirect/");

            Assert.Equal("/me/to/", new Uri(response.Headers["Location"]).AbsolutePath);
        }

    }

}
