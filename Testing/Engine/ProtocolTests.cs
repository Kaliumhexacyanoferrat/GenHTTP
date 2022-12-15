using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ProtocolTests
    {

        private class ValueRecorder : IHandler
        {

            public string? Value { get; private set; }

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                if (request.Content is not null)
                {
                    using var reader = new StreamReader(request.Content);
                    Value = reader.ReadToEnd();
                }

                return new ValueTask<IResponse?>(request.Respond().Build());
            }

        }

        private class ContentLengthResponder : IHandler
        {

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
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
        [TestMethod]
        public async Task TestPost()
        {
            var recorder = new ValueRecorder();

            var str = "From client with ❤";

            using var runner = TestRunner.Run(recorder.Wrap());

            var request = runner.GetRequest();

            request.Method = HttpMethod.Post;
            request.Content = new StringContent(str);

            using var _ = await runner.GetResponse(request);

            Assert.AreEqual(str, recorder.Value);
        }

        /// <summary>
        /// As a client I can submit large data.
        /// </summary>
        [TestMethod]
        public async Task TestPutLarge()
        {
            using var runner = TestRunner.Run(new ContentLengthResponder());

            using var content = new MemoryStream();

            var random = new Random();

            var buffer = new byte[65536];

            for (int i = 0; i < 20; i++) // 1.3 MB
            {
                random.NextBytes(buffer);
                content.Write(buffer, 0, buffer.Length);
            }

            content.Seek(0, SeekOrigin.Begin);

            var request = runner.GetRequest();

            request.Method = HttpMethod.Put;
            request.Content = new StreamContent(content);

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("1310720", await response.GetContent());
        }

    }

}
