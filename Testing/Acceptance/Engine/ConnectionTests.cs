using System.Net;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;

using Resource = GenHTTP.Modules.IO.Resource;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ConnectionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClose(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get((IRequest r) =>
                            {
                                var resource = Resource.FromString("Hello World").Build();

                                return r.Respond()
                                        .Content(resource)
                                        .Connection(Connection.Close)
                                        .Build();
                            });

        await using var runner = await TestHost.RunAsync(handler, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("close", response.GetHeader("Connection")?.ToLower());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUpgrade(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get((IRequest r) =>
                            {
                                return r.Respond()
                                        .Content(new UpgradedContent())
                                        .Status(ResponseStatus.SwitchingProtocols)
                                        .Connection(Connection.Upgrade)
                                        .Build();
                            });

        await using var runner = await TestHost.RunAsync(handler, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.SwitchingProtocols);

        // upgrade responses must not have been chunked encoded
        Assert.IsNull(response.GetHeader("Transfer-Encoding"));

        // upgrade responses do not have a content length
        Assert.IsNull(response.GetContentHeader("Content-Length"));

        // upgrade responses cannot be compressed
        Assert.IsNull(response.GetContentHeader("Content-Encoding"));

        // upgrade responses cannot have eTags
        Assert.IsNull(response.GetHeader("ETag"));

        // this should not work, but it somehow does
        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    #region Supporting data structures

    private class UpgradedContent : IResponseContent
    {

        public ulong? Length => null;

        public ValueTask<ulong?> CalculateChecksumAsync() => new();

        public ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            target.Write("Hello World"u8);
            target.Flush();

            return ValueTask.CompletedTask;
        }

    }

    #endregion

}
