using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

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

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                Cookies = request.Cookies;

                return request.Respond()
                              .Cookie(new Cookie("TestCookie", "TestValue", 86400))
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

            Assert.AreEqual("TestCookie=TestValue; Max-Age=86400; Path=/", response.GetHeader("Set-Cookie"));
        }

        /// <summary>
        /// As a developer, I want to be able to read cookies from the client.
        /// </summary>
        [TestMethod]
        public async Task TestCookiesCanBeRead()
        {
            var provider = new TestProvider();

            using var runner = TestRunner.Run(provider);

            var request = runner.GetRequest();
            request.Headers.Add("Cookie", "1=2; 3=4");

            using var _ = await runner.GetResponse(request);

            Assert.AreEqual("4", provider.Cookies?["3"].Value);
        }

    }

}
