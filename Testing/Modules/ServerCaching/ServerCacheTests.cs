using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
        public async Task TestContentIsInvalidated()
        {
            var file = Path.GetTempFileName();

            try
            {
                using var runner = TestRunner.Run(Content.From(Resource.FromFile(file))
                                                         .Add(ServerCache.Memory()));

                FileUtil.WriteText(file, "1");

                using var first = await runner.GetResponse();

                await first.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("1", await first.GetContent());

                FileUtil.WriteText(file, "12");

                using var second = await runner.GetResponse();

                await second.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("12", await second.GetContent());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public async Task TestContentNotInvalidated()
        {
            var file = Path.GetTempFileName();

            try
            {
                using var runner = TestRunner.Run(Content.From(Resource.FromFile(file))
                                                         .Add(ServerCache.Memory().Invalidate(false)));

                FileUtil.WriteText(file, "1");

                using var first = await runner.GetResponse();

                await first.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("1", await first.GetContent());

                FileUtil.WriteText(file, "12");

                using var second = await runner.GetResponse();

                await second.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("1", await second.GetContent());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public async Task TestVariationRespected()
        {
            var file = Path.GetTempFileName();

            FileUtil.WriteText(file, "This is some content!");

            try
            {
                using var runner = TestRunner.Run(Content.From(Resource.FromFile(file).Type(new FlexibleContentType(ContentType.TextPlain)))
                                                         .Add(CompressedContent.Default())
                                                         .Add(ServerCache.Memory().Invalidate(false)), false);

                var gzipRequest = runner.GetRequest();

                gzipRequest.Headers.Add("Accept-Encoding", "gzip");

                using var gzipResponse = await runner.GetResponse(gzipRequest);

                await gzipResponse.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("gzip", gzipResponse.GetContentHeader("Content-Encoding"));

                var brRequest = runner.GetRequest();

                brRequest.Headers.Add("Accept-Encoding", "br");

                using var brResponse = await runner.GetResponse(brRequest);

                await brResponse.AssertStatusAsync(HttpStatusCode.OK);
                Assert.AreEqual("br", brResponse.GetContentHeader("Content-Encoding"));

                using var uncompressedResponse = await runner.GetResponse();

                await uncompressedResponse.AssertStatusAsync(HttpStatusCode.OK);
                AssertX.IsNullOrEmpty(uncompressedResponse.GetContentHeader("Content-Encoding"));
                Assert.AreEqual("This is some content!", await uncompressedResponse.GetContent());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public async Task TestHeadersPreserved()
        {
            var now = DateTime.UtcNow;

            var handler = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond()
                        .Cookie(new Cookie("CKey", "CValue"))
                        .Header("HKey", "HValue")
                        .Content(Resource.FromString("0123456789").Type(new(ContentType.AudioWav)).Build())
                        .Length(10)
                        .Encoding("some-encoding")
                        .Expires(now)
                        .Modified(now)
                        .Build();
            });

            using var runner = TestRunner.Run(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false);

            using var _ = await runner.GetResponse();

            using var cached = await runner.GetResponse();

            Assert.AreEqual("audio/wav", cached.GetContentHeader("Content-Type"));

            Assert.AreEqual("HValue", cached.GetHeader("HKey"));
            Assert.AreEqual("10", cached.GetContentHeader("Content-Length"));
            Assert.AreEqual("some-encoding", cached.GetContentHeader("Content-Encoding"));

            Assert.AreEqual(now.ToString(), cached.Content.Headers.LastModified.GetValueOrDefault().UtcDateTime.ToString());
            Assert.IsTrue(cached.GetContentHeader("Expires") != null);

            Assert.AreEqual("0123456789", await cached.GetContent());
        }

        [TestMethod]
        public async Task TestNoContentCached()
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

            using var _ = await runner.GetResponse();

            using var __ = await runner.GetResponse();

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public async Task TestNotOkNotCached()
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

            using var _ = await runner.GetResponse();

            using var __ = await runner.GetResponse();

            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public async Task TestPredicateNoMatchNoCache()
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
                                   .Predicate((_, r) => r.ContentType?.KnownType != ContentType.TextHtml)
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = await runner.GetResponse();

            using var __ = await runner.GetResponse();

            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public async Task TestPredicateMatchCached()
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
                                   .Predicate((_, r) => r.ContentType?.KnownType != ContentType.TextHtml)
                                   .Invalidate(false);

            using var runner = TestRunner.Run(handler.Wrap().Add(cache), false);

            using var _ = await runner.GetResponse();

            using var __ = await runner.GetResponse();

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public async Task TestQueryDifferenceNotCached()
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

            using var _ = await runner.GetResponse("/?a=1");

            using var __ = await runner.GetResponse("/?a=2");

            Assert.AreEqual(2, i);
        }

        [TestMethod]
        public async Task TestPostNotCached()
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

            using var _ = await runner.GetResponse(GetPostRequest(runner));

            using var __ = await runner.GetResponse(GetPostRequest(runner));

            Assert.AreEqual(2, i);
        }

        private static HttpRequestMessage GetPostRequest(TestRunner runner)
        {
            var request = runner.GetRequest();

            request.Method = HttpMethod.Post;
            request.Content = new StringContent("Hello World!");

            return request;
        }

        [TestMethod]
        public async Task TestVariationCached()
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

            using var _ = await runner.GetResponse(GetVaryRequest(runner));

            using var __ = await runner.GetResponse(GetVaryRequest(runner));

            Assert.AreEqual(1, i);
        }

        private static HttpRequestMessage GetVaryRequest(TestRunner runner)
        {
            var request = runner.GetRequest();

            request.Headers.Add("Key", "Value");

            return request;
        }

        [TestMethod]
        public async Task TestContent()
        {
            var cache = ServerCache.Memory();

            var content = Resources.From(ResourceTree.FromAssembly("Resources"))
                                   .Add(cache);

            var app = Layout.Create()
                            .Add(Sitemap.FILE_NAME, Sitemap.Create())
                            .Add("app", content);

            using var runner = TestRunner.Run(app, false);

            using var response = await runner.GetResponse("/sitemap.xml");

            var sitemap = await response.GetSitemap();

            AssertX.Contains("/app/Template.html", sitemap);
        }

        [TestMethod]
        public async Task TestDifferentStorageBackends()
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

                using var _ = await runner.GetResponse();

                using var __ = await runner.GetResponse();

                Assert.AreEqual(1, i);
            }
        }

        [TestMethod]
        public async Task TestAccessExpiration()
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

            using var _ = await runner.GetResponse();

            using var __ = await runner.GetResponse();

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
