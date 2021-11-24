using System.IO;
using System.Net;
using System.Text;

using GenHTTP.Api.Protocol;

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
        public void TestGetRoot()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => 42));

            using var response = host.GetResponse();

            Assert.AreEqual("42", response.GetContent());
        }

        [TestMethod]
        public void TestGetPath()
        {
            using var host = TestRunner.Run(Inline.Create().Get("/blubb", () => 42));

            using var response = host.GetResponse("/blubb");

            Assert.AreEqual("42", response.GetContent());
        }

        [TestMethod]
        public void TestGetQueryParam()
        {
            using var host = TestRunner.Run(Inline.Create().Get((int param) => param + 1));

            using var response = host.GetResponse("/?param=41");

            Assert.AreEqual("42", response.GetContent());
        }

        [TestMethod]
        public void TestGetPathParam()
        {
            using var host = TestRunner.Run(Inline.Create().Get(":param", (int param) => param + 1));

            using var response = host.GetResponse("/41");

            Assert.AreEqual("42", response.GetContent());
        }

        [TestMethod]
        public void TestNotFound()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => 42));

            using var response = host.GetResponse("/nope");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestRaw()
        {
            using var host = TestRunner.Run(Inline.Create().Get((IRequest request) =>
            {
                return request.Respond()
                              .Status(ResponseStatus.OK)
                              .Content("42");
            }));

            using var response = host.GetResponse();

            Assert.AreEqual("42", response.GetContent());
        }

        [TestMethod]
        public void TestStream()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => new MemoryStream(Encoding.UTF8.GetBytes("42"))));

            using var response = host.GetResponse();

            Assert.AreEqual("42", response.GetContent());
        }

        [TestMethod]
        public void TestJson()
        {
            using var host = TestRunner.Run(Inline.Create().Get(() => new MyClass("42", 42, 42.0)));

            using var response = host.GetResponse();

            Assert.AreEqual("{\"string\":\"42\",\"int\":42,\"double\":42}", response.GetContent());
        }

        [TestMethod]
        public void TestAsync()
        {
            using var host = TestRunner.Run(Inline.Create().Get(async () =>
            {
                var stream = new MemoryStream();

                await stream.WriteAsync(Encoding.UTF8.GetBytes("42"));

                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }));

            using var response = host.GetResponse();

            Assert.AreEqual("42", response.GetContent());
        }

    }

}
