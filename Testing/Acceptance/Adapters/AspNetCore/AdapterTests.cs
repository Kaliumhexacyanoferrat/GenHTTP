using System.Net;

using GenHTTP.Adapters.AspNetCore;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace GenHTTP.Testing.Acceptance.Adapters.AspNetCore;

[TestClass]
public sealed class AdapterTests
{

    #region Tests

    [TestMethod]
    public async Task TestMap()
    {
        var handler = Content.From(Resource.FromString("Hello World!"));

        var port = TestHost.NextPort();

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls($"http://localhost:{port}");

        await using var app = builder.Build();

        app.Map("/files", handler);

        // explicit fallback, rather than relying on whatever ASP.NET Core happens to
        // do by default when nothing in the pipeline handles a request
        app.Run(context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        });

        await app.StartAsync();

        using var client = TestHost.GetClient();

        using var mapped = await client.GetAsync($"http://localhost:{port}/files");

        await mapped.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Hello World!", await mapped.GetContentAsync());

        using var unmapped = await client.GetAsync($"http://localhost:{port}/other");

        await unmapped.AssertStatusAsync(HttpStatusCode.NotFound);

        await app.StopAsync();
    }

    [TestMethod]
    public async Task TestRun()
    {
        var handler = Content.From(Resource.FromString("Hello World!")).Build();

        var port = TestHost.NextPort();

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls($"http://localhost:{port}");

        await using var app = builder.Build();

        app.Run(handler);

        await app.StartAsync();

        using var client = TestHost.GetClient();

        using var response = await client.GetAsync($"http://localhost:{port}/whatever/path");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());

        await app.StopAsync();
    }

    [TestMethod]
    public async Task TestRunNotFound()
    {
        var handler = Layout.Create().Build();

        var port = TestHost.NextPort();

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls($"http://localhost:{port}");

        await using var app = builder.Build();

        app.Run(handler);

        await app.StartAsync();

        using var client = TestHost.GetClient();

        using var response = await client.GetAsync($"http://localhost:{port}/");

        // an empty Layout has no index handler and returns null for an unmatched request -
        // the adapter is responsible for turning that into a 404 (the engines never see a
        // raw, unwrapped handler like this - their root handler is always wrapped by
        // CoreRouter/ErrorHandler, which never returns null)
        await response.AssertStatusAsync(HttpStatusCode.NotFound);

        await app.StopAsync();
    }

    #endregion

}
