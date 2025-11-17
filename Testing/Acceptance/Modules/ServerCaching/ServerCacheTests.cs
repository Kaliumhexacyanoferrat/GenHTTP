using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Caching;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.ServerCaching;
using GenHTTP.Modules.ServerCaching.Provider;
using GenHTTP.Testing.Acceptance.Utilities;
using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Modules.ServerCaching;

[TestClass]
public class ServerCacheTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentIsInvalidated(TestEngine engine)
    {
        var file = Path.GetTempFileName();

        try
        {
            await using var runner = await TestHost.RunAsync(Content.From(Resource.FromFile(file))
                                                   .Add(ServerCache.Memory()), engine: engine);

            await FileUtil.WriteTextAsync(file, "1");

            using var first = await runner.GetResponseAsync();

            await first.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("1", await first.GetContentAsync());

            await FileUtil.WriteTextAsync(file, "12");

            using var second = await runner.GetResponseAsync();

            await second.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("12", await second.GetContentAsync());
        }
        finally
        {
            try { File.Delete(file); }
            catch
            { /* nop */
            }
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentNotInvalidated(TestEngine engine)
    {
        var file = Path.GetTempFileName();

        try
        {
            await using var runner = await TestHost.RunAsync(Content.From(Resource.FromFile(file))
                                                   .Add(ServerCache.Memory().Invalidate(false)), engine: engine);

            await FileUtil.WriteTextAsync(file, "1");

            using var first = await runner.GetResponseAsync();

            await first.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("1", await first.GetContentAsync());

            await FileUtil.WriteTextAsync(file, "12");

            using var second = await runner.GetResponseAsync();

            await second.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("1", await second.GetContentAsync());
        }
        finally
        {
            try { File.Delete(file); }
            catch
            { /* nop */
            }
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVariationRespected(TestEngine engine)
    {
        var file = Path.GetTempFileName();

        await FileUtil.WriteTextAsync(file, CreateLargeString());

        try
        {
            await using var runner = await TestHost.RunAsync(Content.From(Resource.FromFile(file).Type(new FlexibleContentType(ContentType.TextPlain)))
                                                   .Add(CompressedContent.Default())
                                                   .Add(ServerCache.Memory().Invalidate(false)), false, engine: engine);

            var gzipRequest = runner.GetRequest();

            gzipRequest.Headers.Add("Accept-Encoding", "gzip");

            using var gzipResponse = await runner.GetResponseAsync(gzipRequest);

            await gzipResponse.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("gzip", gzipResponse.GetContentHeader("Content-Encoding"));

            var brRequest = runner.GetRequest();

            brRequest.Headers.Add("Accept-Encoding", "br");

            using var brResponse = await runner.GetResponseAsync(brRequest);

            await brResponse.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("br", brResponse.GetContentHeader("Content-Encoding"));

            using var uncompressedResponse = await runner.GetResponseAsync();

            await uncompressedResponse.AssertStatusAsync(HttpStatusCode.OK);
            AssertX.IsNullOrEmpty(uncompressedResponse.GetContentHeader("Content-Encoding"));
            Assert.AreEqual(CreateLargeString(), await uncompressedResponse.GetContentAsync());
        }
        finally
        {
            try { File.Delete(file); }
            catch
            { /* nop */
            }
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHeadersPreserved(TestEngine engine)
    {
        var now = DateTime.UtcNow;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            return r.Respond()
                    .Cookie(new Cookie("CKey", "CValue"))
                    .Header("HKey", "HValue")
                    .Content(Resource.FromString("0123456789").Type(new FlexibleContentType(ContentType.AudioWav)).Build())
                    .Length(10)
                    .Encoding("some-encoding")
                    .Expires(now)
                    .Modified(now)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false, engine: engine);

        using var _ = await runner.GetResponseAsync();

        using var cached = await runner.GetResponseAsync();

        Assert.AreEqual("audio/wav", cached.GetContentHeader("Content-Type"));

        Assert.AreEqual("HValue", cached.GetHeader("HKey"));
        Assert.AreEqual("10", cached.GetContentHeader("Content-Length"));
        Assert.AreEqual("some-encoding", cached.GetContentHeader("Content-Encoding"));

        Assert.AreEqual(now.ToString(), cached.Content.Headers.LastModified.GetValueOrDefault().UtcDateTime.ToString());
        Assert.IsNotNull(cached.GetContentHeader("Expires"));

        Assert.AreEqual("0123456789", await cached.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoContentCached(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            i++;

            return r.Respond()
                    .Status(ResponseStatus.NoContent)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false, engine: engine);

        using var _ = await runner.GetResponseAsync();

        using var __ = await runner.GetResponseAsync();

        Assert.AreEqual(1, i);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotOkNotCached(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            i++;

            return r.Respond()
                    .Status(ResponseStatus.InternalServerError)
                    .Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(ServerCache.Memory().Invalidate(false)), false, engine: engine);

        using var _ = await runner.GetResponseAsync();

        using var __ = await runner.GetResponseAsync();

        Assert.AreEqual(2, i);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPredicateNoMatchNoCache(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
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

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(cache), false, engine: engine);

        using var _ = await runner.GetResponseAsync();

        using var __ = await runner.GetResponseAsync();

        Assert.AreEqual(2, i);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPredicateMatchCached(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
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

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(cache), false, engine: engine);

        using var _ = await runner.GetResponseAsync();

        using var __ = await runner.GetResponseAsync();

        Assert.AreEqual(1, i);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestQueryDifferenceNotCached(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            i++;

            return r.Respond()
                    .Content(Resource.FromString("0123456789").Build())
                    .Build();
        });

        var cache = ServerCache.Memory()
                               .Invalidate(false);

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(cache), false, engine: engine);

        using var _ = await runner.GetResponseAsync("/?a=1");

        using var __ = await runner.GetResponseAsync("/?a=2");

        Assert.AreEqual(2, i);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPostNotCached(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            i++;

            return r.Respond()
                    .Build();
        });

        var cache = ServerCache.Memory()
                               .Invalidate(false);

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(cache), false, engine: engine);

        using var _ = await runner.GetResponseAsync(GetPostRequest(runner));

        using var __ = await runner.GetResponseAsync(GetPostRequest(runner));

        Assert.AreEqual(2, i);
    }

    private static HttpRequestMessage GetPostRequest(TestHost runner)
    {
        var request = runner.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent("Hello World!");

        return request;
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVariationCached(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            i++;

            return r.Respond()
                    .Header("Vary", "Key")
                    .Build();
        });

        var cache = ServerCache.Memory()
                               .Invalidate(false);

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(cache), false, engine: engine);

        using var _ = await runner.GetResponseAsync(GetVaryRequest(runner));

        using var __ = await runner.GetResponseAsync(GetVaryRequest(runner));

        Assert.AreEqual(1, i);
    }

    private static HttpRequestMessage GetVaryRequest(TestHost runner)
    {
        var request = runner.GetRequest();

        request.Headers.Add("Key", "Value");

        return request;
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDifferentStorageBackends(TestEngine engine)
    {
        foreach (var serverCache in GetBackends())
        {
            var i = 0;

            var handler = new FunctionalHandler(responseProvider: r =>
            {
                i++;

                return r.Respond()
                        .Content(Resource.FromString("0123456789").Build())
                        .Build();
            });

            await using var runner = await TestHost.RunAsync(handler.Wrap().Add(serverCache.Invalidate(false)), false, engine: engine);

            using var _ = await runner.GetResponseAsync();

            using var __ = await runner.GetResponseAsync();

            Assert.AreEqual(1, i);
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAccessExpiration(TestEngine engine)
    {
        var i = 0;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            i++;

            return r.Respond()
                    .Content(Resource.FromString(Guid.NewGuid().ToString()).Build())
                    .Build();
        });

        var meta = Cache.Memory<CachedResponse>();

        var data = Cache.TemporaryFiles<Stream>()
                        .AccessExpiration(TimeSpan.FromMilliseconds(0));

        await using var runner = await TestHost.RunAsync(handler.Wrap().Add(ServerCache.Create(meta, data)), false, engine: engine);

        using var _ = await runner.GetResponseAsync();

        using var __ = await runner.GetResponseAsync();

        Assert.AreEqual(2, i);
    }

    private static string CreateLargeString(int length = 512)
    {
        return new string('A', length);
    }

    private static ServerCacheHandlerBuilder[] GetBackends()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        return
        [
            ServerCache.Memory(),
            ServerCache.TemporaryFiles(),
            ServerCache.Persistent(tempDir)
        ];
    }
}
