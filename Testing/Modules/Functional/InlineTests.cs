using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Functional
{

    [TestClass]
    public sealed class InlineTests
    {

        #region Supporting data structures

        public record MyClass(string String, int Int, double Double);

        #endregion

        [TestMethod]
        public async Task TestGetRoot()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => 42));

            using var response = await host.GetResponse();

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestGetPath()
        {
            using var host = TestRunner.Run(Inline.Create().Get("/blubb", () => 42));

            using var response = await host.GetResponse("/blubb");

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestGetQueryParam()
        {
            using var host = TestRunner.Run(Inline.Create().Get((int param) => param + 1));

            using var response = await host.GetResponse("/?param=41");

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestGetPathParam()
        {
            using var host = TestRunner.Run(Inline.Create().Get(":param", (int param) => param + 1));

            using var response = await host.GetResponse("/41");

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestNotFound()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => 42));

            using var response = await host.GetResponse("/nope");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestRaw()
        {
            using var host = TestRunner.Run(Inline.Create().Get((IRequest request) =>
            {
                return request.Respond()
                              .Status(ResponseStatus.OK)
                              .Content("42");
            }));

            using var response = await host.GetResponse();

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestStream()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => new MemoryStream(Encoding.UTF8.GetBytes("42"))));

            using var response = await host.GetResponse();

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestJson()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => new MyClass("42", 42, 42.0)));

            using var response = await host.GetResponse();

            Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", await response.GetContent());
        }

        [TestMethod]
        public async Task TestPostJson()
        {
            using var host = TestRunner.Run(Inline.Create().Post((MyClass input) => input));

            var request = host.GetRequest();

            request.Method = HttpMethod.Post;

            request.Content = new StringContent("{\"string\":\"42\",\"int\":42,\"double\":42}", Encoding.UTF8, "application/json");

            using var response = await host.GetResponse(request);

            Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", await response.GetContent());
        }

        [TestMethod]
        public async Task TestAsync()
        {
            using var host = TestRunner.Run(Inline.Create().Get(async () =>
            {
                var stream = new MemoryStream();

                await stream.WriteAsync(Encoding.UTF8.GetBytes("42"));

                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }));

            using var response = await host.GetResponse();

            Assert.AreEqual("42", await response.GetContent());
        }

        [TestMethod]
        public async Task TestHandlerBuilder()
        {
            var target = "https://www.google.de/";

            using var host = TestRunner.Run(Inline.Create().Get(() => Redirect.To(target)));

            using var response = await host.GetResponse();

            Assert.AreEqual(target, response.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestHandler()
        {
            var target = "https://www.google.de/";

            using var host = TestRunner.Run(Inline.Create().Get((IHandler parent) => Redirect.To(target).Build(parent)));

            using var response = await host.GetResponse();

            Assert.AreEqual(target, response.GetHeader("Location"));
        }

    }

}
