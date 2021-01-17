using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Caching;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.ServerCaching;
using GenHTTP.Modules.ServerCaching.Provider;
using GenHTTP.Modules.Sitemaps;

using GenHTTP.Testing.Acceptance.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Cookie = GenHTTP.Api.Protocol.Cookie;

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

                using var uncompressedResponse = runner.GetResponse();

                Assert.AreEqual(HttpStatusCode.OK, uncompressedResponse.StatusCode);
                Assert.IsNull(uncompressedResponse.ContentEncoding);
                Assert.AreEqual("This is some content!", uncompressedResponse.GetContent());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public void TestHeadersPreserved()
        {
            var now = DateTime.UtcNow;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Type(new FlexibleContentType(ContentType.AudioWav))
                        .Cookie(new Cookie("CKey", "CValue"))
                        .Header("HKey", "HValue")
                        .Content(Resource.FromString("0123456789").Build())
                        .Length(10)
                        .Encoding("some-encoding")
                        .Expires(now)
                        .Modified(now)
                        .Build();
            });

            using var runner = TestRunner.Run(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false);

            using var _ = runner.GetResponse();

            using var cached = runner.GetResponse();

            Assert.AreEqual("audio/wav", cached.ContentType);

            Assert.AreEqual("HValue", cached.GetResponseHeader("HKey"));
            Assert.AreEqual("10", cached.GetResponseHeader("Content-Length"));
            Assert.AreEqual("some-encoding", cached.GetResponseHeader("Content-Encoding"));

            Assert.AreEqual(now.ToString(), cached.LastModified.ToUniversalTime().ToString());
            Assert.IsTrue(cached.GetResponseHeader("Expires") != null);

            Assert.AreEqual("0123456789", cached.GetContent());
        }

        [TestMethod]
        public void TestNoContentCached()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Status(ResponseStatus.NoContent)
                        .Build();
            });

            using var runner = TestRunner.Run(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false);

            using var _ = runner.GetResponse();

            using var __ = runner.GetResponse();

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestNotOkNotCached()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Status(ResponseStatus.InternalServerError)
                        .Build();
            });

            using var runner = TestRunner.Run(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false);

            using var _ = runner.GetResponse();

            using var __ = runner.GetResponse();

            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public void TestPredicateNoMatchNoCache()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Content(Resource.FromString("0123456789").Build())
                        .Type(new FlexibleContentType(ContentType.TextHtml))
                        .Build();
            });

            var cache = ServerCache.Memory()
                                   .Predicate(r => r.ContentType?.KnownType != ContentType.TextHtml)
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = runner.GetResponse();

            using var __ = runner.GetResponse();

            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public void TestPredicateMatchCached()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Content(Resource.FromString("0123456789").Build())
                        .Type(new FlexibleContentType(ContentType.TextCss))
                        .Build();
            });

            var cache = ServerCache.Memory()
                                   .Predicate(r => r.ContentType?.KnownType != ContentType.TextHtml)
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = runner.GetResponse();

            using var __ = runner.GetResponse();

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestQueryDifferenceNotCached()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Content(Resource.FromString("0123456789").Build())
                        .Build();
            });

            var cache = ServerCache.Memory()
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = runner.GetResponse("/?a=1");

            using var __ = runner.GetResponse("/?a=2");

            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public void TestPostNotCached()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Build();
            });

            var cache = ServerCache.Memory()
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = runner.GetResponse(GetPostRequest(runner));

            using var __ = runner.GetResponse(GetPostRequest(runner));

            Assert.AreEqual(2, i);
        }

        private static HttpWebRequest GetPostRequest(TestRunner runner)
        {
            var request = runner.GetRequest();

            request.Method = "POST";
            request.ContentType = "text/plain";

            using (var requestStream = request.GetRequestStream())
            {
                using var writer = new StreamWriter(requestStream);

                writer.WriteLine("Hello World!");
            }

            return request;
        }

        [TestMethod]
        public void TestVariationCached()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Header("Vary", "Key")
                        .Build();
            });

            var cache = ServerCache.Memory()
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = runner.GetResponse(GetVaryRequest(runner));

            using var __ = runner.GetResponse(GetVaryRequest(runner));

            Assert.AreEqual(1, i);
        }

        private static HttpWebRequest GetVaryRequest(TestRunner runner)
        {
            var request = runner.GetRequest();

            request.Headers.Add("Key", "Value");

            return request;
        }

        [TestMethod]
        public void TestContent()
        {
            var cache = ServerCache.Memory();

            var content = Resources.From(ResourceTree.FromAssembly("Resources"))
                                   .Add(cache);

            var app = Layout.Create()
                            .Add(Sitemap.FILE_NAME, Sitemap.Create())
                            .Add("app", content);

            using var runner = TestRunner.Run(app, false);

            using var response = runner.GetResponse("/sitemap.xml");

            var sitemap = response.GetSitemap();

            AssertX.Contains("/app/Template.html", sitemap);
        }

        [TestMethod]
        public void TestDifferentStorageBackends()
        {
            foreach (var serverCache in GetBackends())
            {
                var i = 0;

                var handler = new FunctionalHandler(responseProvider: (r) =>
                {
                    i++;

                    return r.Respond()
                            .Content(Resource.FromString("0123456789").Build())
                            .Build();
                });

                using var runner = TestRunner.Run(handler.Wrap().Add(serverCache.Invalidate(false)), false);

                using var _ = runner.GetResponse();

                using var __ = runner.GetResponse();

                Assert.AreEqual(1, i);
            }
        }

        [TestMethod]
        public void TestAccessExpiration()
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                i++;

                return r.Respond()
                        .Content(Resource.FromString(Guid.NewGuid().ToString()).Build())
                        .Build();
            });

            var meta = Cache.Memory<CachedResponse>();

            var data = Cache.TemporaryFiles<Stream>()
                            .AccessExpiration(TimeSpan.FromMilliseconds(0));

            using var runner = TestRunner.Run(handler.Wrap().Add(ServerCache.Create(meta, data)), false);

            using var _ = runner.GetResponse();

            using var __ = runner.GetResponse();

            Assert.AreEqual(2, i);
        }

        private static ServerCacheHandlerBuilder[] GetBackends()
        {
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            return new ServerCacheHandlerBuilder[]
            {
                ServerCache.Memory(), 
                ServerCache.TemporaryFiles(), 
                ServerCache.Persistent(tempDir)
            };
        }

    }

}
