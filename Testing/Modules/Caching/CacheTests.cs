﻿using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Caching;
using GenHTTP.Modules.Caching;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Caching
{

    public record CachedEntry(string Data);

    [TestClass]
    public class CacheTests
    {

        [TestMethod]
        public async Task TestHit()
        {
            foreach (var cache in GetCaches<CachedEntry>())
            {
                await cache.StoreAsync("k", "v", new CachedEntry("1"));

                Assert.AreEqual(1, (await cache.GetEntriesAsync("k")).Length);

                Assert.AreEqual("1", (await cache.GetEntryAsync("k", "v"))!.Data);
            }
        }

        [TestMethod]
        public async Task TestMiss()
        {
            foreach (var cache in GetCaches<CachedEntry>())
            {
                Assert.AreEqual(0, (await cache.GetEntriesAsync("k")).Length);

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

                Assert.AreEqual(0, (await cache.GetEntriesAsync("k")).Length);

                Assert.IsNull(await cache.GetEntryAsync("k", "v"));
            }
        }

        [TestMethod]
        public async Task TestStreaming()
        {
            foreach (var cache in GetCaches<Stream>())
            {
                using var stream = new MemoryStream(new byte[] { 1 });

                await cache.StoreAsync("k", "v", stream);

                Assert.AreEqual(1, (await cache.GetEntriesAsync("k")).Length);

                using var resultStream = (await cache.GetEntryAsync("k", "v"))!;

                Assert.AreNotEqual(stream, resultStream);
                Assert.AreEqual(1, resultStream.Length);
            }
        }

        [TestMethod]
        public async Task TestDirectStreaming()
        {
            foreach (var cache in GetCaches<Stream>())
            {
                await cache.StoreDirectAsync("k", "v", (s) => s.WriteAsync(new byte[] { 1 }));

                Assert.AreEqual(1, (await cache.GetEntriesAsync("k")).Length);

                using var resultStream = (await cache.GetEntryAsync("k", "v"))!;

                Assert.AreEqual(1, resultStream.Length);
            }
        }

        [TestMethod]
        public async Task TestStreamingMiss()
        {
            foreach (var cache in GetCaches<Stream>())
            {
                Assert.AreEqual(0, (await cache.GetEntriesAsync("k")).Length);

                Assert.IsNull(await cache.GetEntryAsync("k", "v"));
            }
        }

        [TestMethod]
        public void TestFileCacheDisposable()
        {
            using var _ = Cache.TemporaryFiles<CachedEntry>().Build();
        }

        private ICache<T>[] GetCaches<T>()
        {
            return new ICache<T>[]
            {
                Cache.Memory<T>().Build(),
                Cache.TemporaryFiles<T>().Build()
            };
        }

    }

}