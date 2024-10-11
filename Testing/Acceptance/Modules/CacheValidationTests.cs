﻿using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class CacheValidationTests
{

    [TestMethod]
    public async Task TestETagIsGenerated()
    {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = await runner.GetResponseAsync();

            var eTag = response.GetHeader("ETag");

            Assert.IsNotNull(eTag);

            AssertX.StartsWith("\"", eTag);
            AssertX.EndsWith("\"", eTag);
        }

    [TestMethod]
    public async Task TestServerReturnsUnmodified()
    {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = await runner.GetResponseAsync();

            var eTag = response.GetHeader("ETag");

            var request = runner.GetRequest();

            request.Headers.Add("If-None-Match", eTag);

            using var cached = await runner.GetResponseAsync(request);

            await cached.AssertStatusAsync(HttpStatusCode.NotModified);

            Assert.AreEqual("0", cached.GetContentHeader("Content-Length"));
        }

    [TestMethod]
    public async Task TestServerReturnsModified()
    {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            var request = runner.GetRequest();

            request.Headers.Add("If-None-Match", "\"123\"");

            using var reloaded = await runner.GetResponseAsync(request);

            await reloaded.AssertStatusAsync(HttpStatusCode.OK);
        }

    [TestMethod]
    public async Task TestNoContentNoEtag()
    {
            var noContent = new FunctionalHandler(responseProvider: (r) =>
            {
                return r.Respond().Status(Api.Protocol.ResponseStatus.NoContent).Build();
            });

            using var runner = TestHost.Run(noContent.Wrap());

            using var response = await runner.GetResponseAsync();

            Assert.IsFalse(response.Headers.Contains("ETag"));
        }

    [TestMethod]
    public async Task TestOtherMethodNoETag()
    {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            var request = runner.GetRequest();

            request.Method = HttpMethod.Delete;

            using var response = await runner.GetResponseAsync(request);

            Assert.IsFalse(response.Headers.Contains("ETag"));
        }

}
