﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ParserTests
{

    [TestMethod]
    public async Task TestEndodedUri()
    {
        using var runner = TestHost.Run(new PathReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/söme/ürl/with specialities/");

        Assert.AreEqual("/söme/ürl/with specialities/", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestEncodedQuery()
    {
        using var runner = TestHost.Run(new QueryReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/?söme key=💕");

        Assert.AreEqual("söme key=💕", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestMultipleSlashes()
    {
        using var runner = TestHost.Run(new PathReturner().Wrap());

        using var respose = await runner.GetResponseAsync("//one//two//three//");

        Assert.AreEqual("//one//two//three//", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestEmptyQuery()
    {
        using var runner = TestHost.Run(new QueryReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/?");

        Assert.AreEqual(string.Empty, await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestUnkeyedQuery()
    {
        using var runner = TestHost.Run(new QueryReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/?query");

        Assert.AreEqual("query=", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestQueryWithSlashes()
    {
        using var runner = TestHost.Run(new QueryReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/?key=/one/two");

        Assert.AreEqual("key=/one/two", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestQueryWithSpaces()
    {
        using var runner = TestHost.Run(new QueryReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/?path=/Some+Folder/With%20Subfolders/");

        Assert.AreEqual("path=/Some+Folder/With Subfolders/", await respose.GetContentAsync());
    }

    #region Supporting data structures

    private class PathReturner : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IHandler Parent => throw new NotImplementedException();

        public ValueTask<IResponse?> HandleAsync(IRequest request) => request.Respond()
                                                                             .Content(request.Target.Path.ToString())
                                                                             .BuildTask();
    }

    private class QueryReturner : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IHandler Parent => throw new NotImplementedException();

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            return request.Respond()
                          .Content(string.Join('|', request.Query.Select(kv => kv.Key + "=" + kv.Value)))
                          .BuildTask();
        }
    }

    #endregion

}
