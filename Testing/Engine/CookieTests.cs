using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class CookieTests
    {

        private class TestProvider : IHandler
        {

            public ICookieCollection? Cookies { get; private set; }

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotSupportedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                Cookies = request.Cookies;

                return request.Respond()
                              .Cookie(new Cookie("TestCookie", "TestValue"))
                              .Content("I ❤ Cookies!")
                              .Type(ContentType.TextHtml)
                              .BuildTask();

            }

        }

        /// <summary>
        /// As a developer, I want to be able to set cookies to be accepted by the browser.
        /// </summary>
        [TestMethod]
        public async Task TestCookiesCanBeReturned()
        {
            using var runner = TestRunner.Run(new TestProvider());

            using var response = await runner.GetResponse();

            Assert.AreEqual("TestCookie=TestValue; Path=/", response.GetHeader("Set-Cookie"));
        }

        /// <summary>
        /// As a developer, I want to be able to read cookies from the client.
        /// </summary>
        [TestMethod]
        public async Task TestCookiesCanBeRead()
        {
            var provider = new TestProvider();

            var layout = Layout.Create().Add("test", provider.Wrap());

            using var runner = TestRunner.Run(layout);

            var request = runner.GetRequest();
            request.Headers.Add("Cookie", "1=2; 3=4");

            using var _ = await runner.GetResponse(request);

            Assert.AreEqual("4", provider.Cookies?["3"].Value);
        }

    }

}
