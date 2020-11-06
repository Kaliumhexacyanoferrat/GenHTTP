﻿using System;
using System.Net;
using System.Collections.Generic;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class ResponseTests
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
        [Fact]
        public void TestProperties()
        {
            var provider = new ResponseProvider();

            var router = Layout.Create().Index(provider.Wrap());

            using var runner = TestRunner.Run(router);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal("Hello World", response.GetContent());
            Assert.Equal("text/x-custom", response.ContentType);

            Assert.Equal(provider.Modified.WithoutMS(), response.LastModified.WithoutMS());
            Assert.NotNull(response.Headers["Expires"]);

            Assert.Equal("Test Runner", response.Headers["X-Powered-By"]);
        }

        /// <summary>
        /// As a client, I'd like a response containing an empty body to return a Content-Length of 0.
        /// </summary>
        [Fact]
        public void TestEmptyBody()
        {
            var provider = new ResponseProvider();

            var router = Layout.Create().Index(provider.Wrap());

            using var runner = TestRunner.Run(router);

            var request = runner.GetRequest();
            request.Method = "POST";
            request.KeepAlive = true;

            using var response = request.GetResponse();

            Assert.Equal("", response.ContentType);

            Assert.Equal(0, response.ContentLength);

            Assert.Equal("Keep-Alive", response.Headers["Connection"]);
        }

    }

}
