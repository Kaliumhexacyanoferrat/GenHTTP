using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class CookieTests
    {

        private class TestProvider : IContentProvider
        {

            public ICookieCollection? Cookies { get; private set; }

            public FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextHtml);

            public string? Title => null;

            public IResponseBuilder Handle(IRequest request)
            {
                Cookies = request.Cookies;

                return request.Respond()
                              .Cookie(new Cookie("TestCookie", "TestValue"))
                              .Content(new StringContent("I ❤ Cookies!"))
                              .Type(ContentType!);

            }

        }

        /// <summary>
        /// As a developer, I want to be able to set cookies to be accepted by the browser.
        /// </summary>
        [Fact]
        public void TestCookiesCanBeReturned()
        {
            var layout = Layout.Create().Add("test", new TestProvider());

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/test");

            Assert.Equal("TestCookie=TestValue; Path=/", response.Headers["Set-Cookie"]);
        }

        /// <summary>
        /// As a developer, I want to be able to read cookies from the client.
        /// </summary>
        [Fact]
        public void TestCookiesCanBeRead()
        {
            var provider = new TestProvider();

            var layout = Layout.Create().Add("test", provider);

            using var runner = TestRunner.Run(layout);

            var request = runner.GetRequest("/test");
            request.Headers.Add("Cookie", "1=2; 3=4");

            using var _ = request.GetSafeResponse();

            Assert.Equal("4", provider.Cookies?["3"]?.Value);
        }

    }

}
