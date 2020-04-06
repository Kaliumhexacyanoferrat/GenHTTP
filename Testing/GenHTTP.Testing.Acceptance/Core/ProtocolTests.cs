using System;
using System.IO;
using System.Text;

using Xunit;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Api.Content;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class ProtocolTests
    {

        private class TestExtension : IServerExtension
        {

            public string? Value { get; private set; }

            public IContentProvider? Intercept(IRequest request) => null;

            public void Intercept(IRequest request, IResponse response)
            {
                if (request.Content != null)
                {
                    using var reader = new StreamReader(request.Content);
                    Value = reader.ReadToEnd();
                }
            }

        }

        private class ContentLengthResponder : IContentProvider
        {

            public string? Title => "Content Length Responder";

            public FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextPlain);

            public IResponseBuilder Handle(IRequest request)
            {
                var content = request.Content?.Length.ToString() ?? "No Content";

                return request.Respond()
                              .Content(content)
                              .Type(Api.Protocol.ContentType.TextPlain);
            }

        }

        /// <summary>
        /// As a client I can stream data to the server.
        /// </summary>
        [Fact]
        public void TestPost()
        {
            var test = new TestExtension();

            var str = "From client with ❤";

            using var runner = new TestRunner();

            runner.Host.Extension(test).Start();

            var request = runner.GetRequest();
            request.Method = "POST";

            using (var input = request.GetRequestStream())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                input.Write(bytes, 0, bytes.Length);
            }

            using var __ = request.GetSafeResponse();

            Assert.Equal(str, test.Value);
        }

        /// <summary>
        /// As a client I can submit large data.
        /// </summary>
        [Fact]
        public void TestPutLarge()
        {
            using var runner = TestRunner.Run(Layout.Create().Add("test", new ContentLengthResponder()));

            var request = runner.GetRequest("/test");
            request.Method = "PUT";

            using (var input = request.GetRequestStream())
            {
                var random = new Random();

                var buffer = new byte[65536];

                for (int i = 0; i < 20; i++) // 1.3 MB
                {
                    random.NextBytes(buffer);
                    input.Write(buffer, 0, buffer.Length);
                }
            }

            using var response = request.GetSafeResponse();

            Assert.Equal("1310720", response.GetContent());
        }
        
    }

}
