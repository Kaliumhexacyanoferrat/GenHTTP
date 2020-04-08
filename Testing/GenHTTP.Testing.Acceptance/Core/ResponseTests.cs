using System;
using System.IO;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;
using System.Collections.Generic;

namespace GenHTTP.Testing.Acceptance.Core
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

            public IResponse Handle(IRequest request)
            {
                return request.Respond()
                              .Content("Hello World")
                              .Type("text/x-custom")
                              .Expires(DateTime.Now.AddYears(1))
                              .Modified(Modified)
                              .Header("X-Powered-By", "Test Runner")
                              .Build();
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

    }

}
