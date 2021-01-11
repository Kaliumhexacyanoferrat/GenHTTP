using System.IO;
using System.Net;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.ServerCaching;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ServerCaching
{

    [TestClass]
    public class ServerCacheTests
    {

        [TestMethod]
        public void TestContentIsInvalidated()
        {
            var file = Path.GetTempFileName();

            try
            {
                using var runner = TestRunner.Run(Content.From(Resource.FromFile(file))
                                                         .Add(ServerCache.Memory()));

                File.WriteAllText(file, "1");

                using var first = runner.GetResponse();

                Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
                Assert.AreEqual("1", first.GetContent());

                File.WriteAllText(file, "2");

                using var second = runner.GetResponse();

                Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
                Assert.AreEqual("2", second.GetContent());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public void TestContentNotInvalidated()
        {
            var file = Path.GetTempFileName();

            try
            {
                using var runner = TestRunner.Run(Content.From(Resource.FromFile(file))
                                                         .Add(ServerCache.Memory().Invalidate(false)));

                File.WriteAllText(file, "1");

                using var first = runner.GetResponse();

                Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
                Assert.AreEqual("1", first.GetContent());

                File.WriteAllText(file, "2");

                using var second = runner.GetResponse();

                Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
                Assert.AreEqual("1", second.GetContent());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public void TestVariationRespected()
        {
            var file = Path.GetTempFileName();

            File.WriteAllText(file, "This is some content!");

            try
            {
                using var runner = TestRunner.Run(Content.From(Resource.FromFile(file).Type(new FlexibleContentType(ContentType.TextPlain)))
                                                         .Add(CompressedContent.Default())
                                                         .Add(ServerCache.Memory().Invalidate(false)), false);

                var gzipRequest = runner.GetRequest();

                gzipRequest.Headers.Add("Accept-Encoding", "gzip");

                using var gzipResponse = runner.GetResponse(gzipRequest);

                Assert.AreEqual(HttpStatusCode.OK, gzipResponse.StatusCode);
                Assert.AreEqual("gzip", gzipResponse.GetResponseHeader("Content-Encoding"));

                var brRequest = runner.GetRequest();

                brRequest.Headers.Add("Accept-Encoding", "br");

                using var brResponse = runner.GetResponse(brRequest);

                Assert.AreEqual(HttpStatusCode.OK, brResponse.StatusCode);
                Assert.AreEqual("br", brResponse.GetResponseHeader("Content-Encoding"));
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

    }

}
