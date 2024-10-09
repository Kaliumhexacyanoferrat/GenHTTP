using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ResponseTests
    {

        private class ResponseProvider : IHandler
        {

            public DateTime Modified { get; }

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public ResponseProvider()
            {
                Modified = DateTime.Now.AddDays(-10);
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                return request.Method.KnownMethod switch
                {
                    RequestMethod.POST => request.Respond()
                                                 .Content("")
                                                 .Type("")
                                                 .BuildTask(),
                    _ => request.Respond()
                                .Content("Hello World")
                                .Type("text/x-custom")
                                .Expires(DateTime.Now.AddYears(1))
                                .Modified(Modified)
                                .Header("X-Powered-By", "Test Runner")
                                .BuildTask(),
                };
            }

        }

        /// <summary>
        /// As a developer, I'd like to use all of the response builders methods.
        /// </summary>
        [TestMethod]
        public async Task TestProperties()
        {
            var provider = new ResponseProvider();

            var router = Layout.Create().Index(provider.Wrap());

            using var runner = TestHost.Run(router);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("Hello World", await response.GetContentAsync());
            Assert.AreEqual("text/x-custom", response.GetContentHeader("Content-Type"));

            Assert.AreEqual(provider.Modified.WithoutMS(), response.Content.Headers.LastModified);
            Assert.IsNotNull(response.GetContentHeader("Expires"));

            Assert.AreEqual("Test Runner", response.GetHeader("X-Powered-By"));
        }

        /// <summary>
        /// As a client, I'd like a response containing an empty body to return a Content-Length of 0.
        /// </summary>
        [TestMethod]
        public async Task TestEmptyBody()
        {
            var provider = new ResponseProvider();

            var router = Layout.Create().Index(provider.Wrap());

            using var runner = TestHost.Run(router);

            var request = runner.GetRequest();
            request.Method = HttpMethod.Post;

            using var response = await runner.GetResponseAsync(request);

            AssertX.IsNullOrEmpty(response.GetContentHeader("Content-Type"));

            Assert.AreEqual("0", response.GetContentHeader("Content-Length"));
        }

    }

}
