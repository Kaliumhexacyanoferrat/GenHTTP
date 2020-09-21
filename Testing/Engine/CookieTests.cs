using System;
using System.Collections.Generic;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class CookieTests
    {

        private class TestProvider : IHandler
        {

            public ICookieCollection? Cookies { get; private set; }

            public IHandler Parent => throw new NotSupportedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public IResponse? Handle(IRequest request)
            {
                Cookies = request.Cookies;

                return request.Respond()
                              .Cookie(new Cookie("TestCookie", "TestValue"))
                              .Content("I ❤ Cookies!")
                              .Type(ContentType.TextHtml)
                              .Build();

            }

        }

        /// <summary>
        /// As a developer, I want to be able to set cookies to be accepted by the browser.
        /// </summary>
        [Fact]
        public void TestCookiesCanBeReturned()
        {
            using var runner = TestRunner.Run(new TestProvider());

            using var response = runner.GetResponse();

            Assert.Equal("TestCookie=TestValue; Path=/", response.Headers["Set-Cookie"]);
        }

        /// <summary>
        /// As a developer, I want to be able to read cookies from the client.
        /// </summary>
        [Fact]
        public void TestCookiesCanBeRead()
        {
            var provider = new TestProvider();

            var layout = Layout.Create().Add("test", provider.Wrap());

            using var runner = TestRunner.Run(layout);

            var request = runner.GetRequest("/test");
            request.Headers.Add("Cookie", "1=2; 3=4");

            using var _ = request.GetSafeResponse();

            Assert.Equal("4", provider.Cookies?["3"].Value);
        }

    }

}
