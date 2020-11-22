using System;
using System.Net;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ResponseTests
    {

        private class ResponseProvider : IHandler
        {

            public DateTime Modified { get; }

            public IHandler Parent => throw new NotImplementedException();

            public ResponseProvider()
            {
                Modified = DateTime.Now.AddDays(-10);
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                switch (request.Method.KnownMethod)
                {
                    case RequestMethod.POST:
                        return request.Respond()
                                      .Content("")
                                      .Type("")
                                      .BuildTask();
                    default:
                        return request.Respond()
                                      .Content("Hello World")
                                      .Type("text/x-custom")
                                      .Expires(DateTime.Now.AddYears(1))
                                      .Modified(Modified)
                                      .Header("X-Powered-By", "Test Runner")
                                      .BuildTask();
                }

            }

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// As a developer, I'd like to use all of the response builders methods.
        /// </summary>
        [TestMethod]
        public void TestProperties()
        {
            var provider = new ResponseProvider();

            var router = Layout.Create().Index(provider.Wrap());

            using var runner = TestRunner.Run(router);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual("Hello World", response.GetContent());
            Assert.AreEqual("text/x-custom", response.ContentType);

            Assert.AreEqual(provider.Modified.WithoutMS(), response.LastModified.WithoutMS());
            Assert.IsNotNull(response.Headers["Expires"]);

            Assert.AreEqual("Test Runner", response.Headers["X-Powered-By"]);
        }

        /// <summary>
        /// As a client, I'd like a response containing an empty body to return a Content-Length of 0.
        /// </summary>
        [TestMethod]
        public void TestEmptyBody()
        {
            var provider = new ResponseProvider();

            var router = Layout.Create().Index(provider.Wrap());

            using var runner = TestRunner.Run(router);

            var request = runner.GetRequest();
            request.Method = "POST";
            request.KeepAlive = true;

            using var response = request.GetResponse();

            Assert.AreEqual("", response.ContentType);

            Assert.AreEqual(0, response.ContentLength);

            Assert.AreEqual("Keep-Alive", response.Headers["Connection"]);
        }

    }

}
