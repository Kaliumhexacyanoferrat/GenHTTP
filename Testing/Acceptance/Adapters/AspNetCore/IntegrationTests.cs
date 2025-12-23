using System.Net;

using GenHTTP.Adapters.AspNetCore;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Testing.Acceptance.Adapters.AspNetCore;

[TestClass]
public class IntegrationTests
{

    #region Tests

    [TestMethod]
    public async Task TestMapping()
    {
        var port = TestHost.NextPort();

        var options = (WebApplication app) =>
        {
            app.Map("/builder", Inline.Create().Get("/a", () => "a"));
            app.Map("/handler", Inline.Create().Get("/b", () => "b").Build());
        };

        await using var app = await RunApplicationAsync(port, options);

        using var client = new HttpClient();

        using var builderResponse = await GetResponseAsync(client, "/builder/a", port);

        await builderResponse.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("a", await builderResponse.GetContentAsync());

        using var handlerResponse = await GetResponseAsync(client, "/handler/b", port);

        await handlerResponse.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("b", await handlerResponse.GetContentAsync());
    }

    [TestMethod]
    public async Task TestDefaults()
    {
        var port = TestHost.NextPort();

        var options = (WebApplication app) =>
        {
            app.Map("/content", Content.From(Resource.FromString("Hello World")).Defaults(rangeSupport: true));
        };

        await using var app = await RunApplicationAsync(port, options);

        using var client = new HttpClient();

        using var response = await GetResponseAsync(client, "/content", port);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Hello World", await response.GetContentAsync());

        Assert.IsTrue(response.Headers.Contains("ETag"));
    }

    [TestMethod]
    public async Task TestErrorHandling()
    {
        var port = TestHost.NextPort();

        var options = (WebApplication app) =>
        {
            app.Map("/notfound", Inline.Create().Defaults());
        };

        await using var app = await RunApplicationAsync(port, options);

        using var client = new HttpClient();

        using var response = await GetResponseAsync(client, "/notfound", port);

        await response.AssertStatusAsync(HttpStatusCode.NotFound);

        AssertX.Contains("404", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestImplicitServer()
    {
        var port = TestHost.NextPort();

        var options = (WebApplication app) =>
        {
            app.Map("/server", Inline.Create().Get(async (IRequest r) =>
            {
                var server = r.Server;

                await server.DisposeAsync(); // nop

                await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await server.StartAsync());

                Assert.IsTrue(server.Running);
                Assert.IsFalse(server.Development);

                Assert.Contains(".NET", r.Server.Version);

                Assert.IsNotNull(r.Server.Handler);

                Assert.IsNotNull(r.EndPoint.Address);
                Assert.IsFalse(r.EndPoint.Secure);

                r.EndPoint.Dispose(); // nop
            }));
        };

        await using var app = await RunApplicationAsync(port, options);

        using var client = new HttpClient();

        using var response = await GetResponseAsync(client, "/server", port);

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    #endregion

    #region Helpers

    private static async ValueTask<WebApplication> RunApplicationAsync(int port, Action<WebApplication> options)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();

        builder.WebHost.ConfigureKestrel(o =>
        {
            o.AllowSynchronousIO = true;
            o.Listen(IPAddress.Any, port);
        });

        var app = builder.Build();

        options.Invoke(app);

        await app.StartAsync();

        return app;
    }

    private static async ValueTask<HttpResponseMessage> GetResponseAsync(HttpClient client, string path, int port)
        => await client.GetAsync($"http://localhost:{port}{path}");

    #endregion

}
