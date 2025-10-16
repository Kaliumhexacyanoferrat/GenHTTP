using GenHTTP.Api.Content.Caching;
using GenHTTP.Modules.Caching;

namespace GenHTTP.Testing.Acceptance.Modules.Caching;

public record CachedEntry(string Data, DateTime? Date = null, int? Int = null);

[TestClass]
public class CacheTests
{

    [TestMethod]
    public async Task TestHit()
    {
        foreach (var cache in GetCaches<CachedEntry>())
        {
            var now = DateTime.Now;

            await cache.StoreAsync("k", "v", new CachedEntry("1", now, 42));

            Assert.HasCount(1, await cache.GetEntriesAsync("k"));

            var hit = (await cache.GetEntryAsync("k", "v"))!;

            Assert.AreEqual("1", hit.Data);
            Assert.AreEqual(now, hit.Date);
            Assert.AreEqual(42, hit.Int);
        }
    }

    [TestMethod]
    public async Task TestMiss()
    {
        foreach (var cache in GetCaches<CachedEntry>())
        {
            Assert.IsEmpty(await cache.GetEntriesAsync("k"));

            Assert.IsNull(await cache.GetEntryAsync("k", "v"));
        }
    }

    [TestMethod]
    public async Task TestVariantMiss()
    {
        foreach (var cache in GetCaches<CachedEntry>())
        {
            await cache.StoreAsync("k", "v1", new CachedEntry("1"));

            Assert.IsNull(await cache.GetEntryAsync("k", "v2"));
        }
    }

    [TestMethod]
    public async Task TestRemoval()
    {
        foreach (var cache in GetCaches<CachedEntry>())
        {
            await cache.StoreAsync("k", "v", new CachedEntry("1"));

            await cache.StoreAsync("k", "v", null);

            Assert.IsEmpty(await cache.GetEntriesAsync("k"));

            Assert.IsNull(await cache.GetEntryAsync("k", "v"));
        }
    }

    [TestMethod]
    public async Task TestStreaming()
    {
        foreach (var cache in GetCaches<Stream>())
        {
            using var stream = new MemoryStream([1]);

            await cache.StoreAsync("k", "v", stream);

            Assert.HasCount(1, await cache.GetEntriesAsync("k"));

            await using var resultStream = (await cache.GetEntryAsync("k", "v"))!;

            Assert.AreNotEqual(stream, resultStream);
            Assert.AreEqual(1, resultStream.Length);
        }
    }

    [TestMethod]
    public async Task TestStreamingOverwrite()
    {
        foreach (var cache in GetCaches<Stream>())
        {
            using var stream = new MemoryStream([1]);

            await cache.StoreAsync("k", "v", stream);

            await cache.StoreAsync("k", "v", stream);

            Assert.HasCount(1, await cache.GetEntriesAsync("k"));
        }
    }

    [TestMethod]
    public async Task TestDirectStreaming()
    {
        foreach (var cache in GetCaches<Stream>())
        {
            await cache.StoreDirectAsync("k", "v", s => s.WriteAsync(new byte[]
            {
                1
            }));

            Assert.HasCount(1, await cache.GetEntriesAsync("k"));

            await using var resultStream = (await cache.GetEntryAsync("k", "v"))!;

            Assert.AreEqual(1, resultStream.Length);
        }
    }

    [TestMethod]
    public async Task TestDirectStreamingOverwrite()
    {
        foreach (var cache in GetCaches<Stream>())
        {
            await cache.StoreDirectAsync("k", "v", s => s.WriteAsync(new byte[]
            {
                1
            }));

            await cache.StoreDirectAsync("k", "v", s => s.WriteAsync(new byte[]
            {
                1
            }));

            Assert.HasCount(1, await cache.GetEntriesAsync("k"));
        }
    }

    [TestMethod]
    public async Task TestStreamingMiss()
    {
        foreach (var cache in GetCaches<Stream>())
        {
            Assert.IsEmpty(await cache.GetEntriesAsync("k"));

            Assert.IsNull(await cache.GetEntryAsync("k", "v"));
        }
    }

    private static ICache<T>[] GetCaches<T>() =>
    [
        Cache.Memory<T>().Build(),
        Cache.TemporaryFiles<T>().Build()
    ];
    
}
