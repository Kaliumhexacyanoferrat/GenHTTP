using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.ServerCaching;

namespace GenHTTP.Testing.Acceptance.Modules.ServerCaching
{

    [TestClass]
    public class PrecompressedContentTest
    {

        [TestMethod]
        public async Task TestContentCanBePreCompressed()
        {
            var content = Resources.From(ResourceTree.FromAssembly("Resources"))
                                   .Add(CompressedContent.Default().Level(CompressionLevel.Optimal))
                                   .Add(ServerCache.Memory());

            using var runner = TestRunner.Run(content, false);

            var request = runner.GetRequest("/Template.html");

            request.Headers.Add("Accept-Encoding", "br");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("br", response.GetHeader("Content-Encoding"));
        }

    }

}
