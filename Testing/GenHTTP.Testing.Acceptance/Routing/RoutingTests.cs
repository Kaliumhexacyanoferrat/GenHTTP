using System;
using System.Net;

using Xunit;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Api.Modules;

namespace GenHTTP.Testing.Acceptance.Routing
{

    public class RoutingTests
    {

        private class ThrowingProvider : ContentProviderBase
        {

            public ThrowingProvider(ResponseModification? modification) : base(modification)
            {

            }

            public override string? Title => null;

            public override FlexibleContentType? ContentType => null;

            protected override IResponseBuilder HandleInternal(IRequest request)
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// As a client, I expect the server to return 404 for non-existing files.
        /// </summary>
        [Fact]
        public void NotFoundForUnknownRoute()
        {
            using var runner = TestRunner.Run();

            using var response = runner.GetResponse();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// As a developer, I would like to customize the error messages generated
        /// by my server.
        /// </summary>
        [Fact]
        public void TestCustomErrorHandler()
        {
            var layout = Layout.Create()
                               .Default(new ThrowingProvider(null))
                               .ErrorHandler(Content.From("Oh misfortune!"));

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();
            Assert.Equal("Oh misfortune!", response.GetContent());
        }

    }

}
