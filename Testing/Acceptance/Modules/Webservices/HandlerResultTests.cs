﻿using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.Webservices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

[TestClass]
public sealed class HandlerResultTests
{

    #region Helpers

    public static IBuilder<IResourceTree> CreateTree()
    {
        var subTree = VirtualTree.Create()
                                 .Add("index.htm", Resource.FromString("Sub Index"))
                                 .Add("my.txt", Resource.FromString("My Textfile"));

        return VirtualTree.Create()
                          .Add("index.html", Resource.FromString("Index"))
                          .Add("sub", subTree);
    }

    #endregion

    #region Supporting data structures

    public sealed class RootService
    {
        private static readonly IBuilder<IResourceTree> Tree = CreateTree();

        [ResourceMethod]
        public IHandlerBuilder Root() => StaticWebsite.From(Tree);
    }

    public sealed class PathService
    {
        private static readonly IBuilder<IResourceTree> Tree = CreateTree();

        [ResourceMethod(path: "/mypath/:pathParam/")]
        public IHandlerBuilder Pathed(string pathParam)
        {
            Assert.AreEqual("param", pathParam);

            return StaticWebsite.From(Tree);
        }
    }

    public sealed class PathAsyncService
    {
        private static readonly IBuilder<IResourceTree> Tree = CreateTree();

        [ResourceMethod(path: "/mypath/:pathParam/")]
        public Task<IHandlerBuilder> Pathed(string pathParam)
        {
            Assert.AreEqual("param", pathParam);

            IHandlerBuilder handlerBuilder = StaticWebsite.From(Tree);

            return Task.FromResult(handlerBuilder);
        }
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRoot(TestEngine engine)
    {
        var app = Layout.Create()
                        .AddService<RootService>("c");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/c/sub/my.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("My Textfile", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPathed(TestEngine engine)
    {
        var app = Layout.Create()
                        .AddService<PathService>("c");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/c/mypath/param/sub/my.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("My Textfile", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPathedAsync(TestEngine engine)
    {
        var app = Layout.Create()
                        .AddService<PathAsyncService>("c");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/c/mypath/param/sub/my.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("My Textfile", await response.GetContentAsync());
    }

    #endregion

}
