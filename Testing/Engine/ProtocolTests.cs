using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Xunit;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Basics;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class ProtocolTests
    {

        private class ValueRecorder : IHandler
        {

            public string? Value { get; private set; }

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                if (request.Content != null)
                {
                    using var reader = new StreamReader(request.Content);
                    Value = reader.ReadToEnd();
                }

                return new ValueTask<IResponse?>(request.Respond().Build());
            }

        }

        private class ContentLengthResponder : IHandler
        {

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                var content = request.Content?.Length.ToString() ?? "No Content";

                return request.Respond()
                              .Content(content)
                              .Type(ContentType.TextPlain)
                              .BuildTask();
            }

        }

        /// <summary>
        /// As a client I can stream data to the server.
        /// </summary>
        [Fact]
        public void TestPost()
        {
            var recorder = new ValueRecorder();

            var str = "From client with ❤";

            using var runner = TestRunner.Run(recorder.Wrap());

            var request = runner.GetRequest();
            request.Method = "POST";

            using (var input = request.GetRequestStream())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                input.Write(bytes, 0, bytes.Length);
            }

            using var __ = request.GetSafeResponse();

            Assert.Equal(str, recorder.Value);
        }

        /// <summary>
        /// As a client I can submit large data.
        /// </summary>
        [Fact]
        public void TestPutLarge()
        {
            using var runner = TestRunner.Run(new ContentLengthResponder());

            var request = runner.GetRequest();
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
