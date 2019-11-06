using System;
using System.IO;
using System.Text;

using Xunit;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Api.Modules;

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

        /// <summary>
        /// As a client I can stream data to the server.
        /// </summary>
        [Fact]
        public void TestPost()
        {
            var test = new TestExtension();

            var str = "From client with ❤";

            using var runner = new TestRunner();

            using var _ = runner.Builder.Extension(test).Build();

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
            using var runner = TestRunner.Run(Layout.Create().Add("test", Content.From("Yes!")));

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

            Assert.Equal("Yes!", response.GetContent());
        }

    }

}
