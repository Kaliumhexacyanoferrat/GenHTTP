using System.Web;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ParserTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEndodedUri(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(new PathReturner().Wrap(), engine: engine);

        using var respose = await runner.GetResponseAsync("/söme/ürl/with specialities/");

        Assert.AreEqual("/söme/ürl/with specialities/", await respose.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEncodedQuery(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(new QueryReturner().Wrap(), engine: engine);

        using var respose = await runner.GetResponseAsync("/?söme key=💕");

        Assert.AreEqual("söme key=💕", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestMultipleSlashes()
    {
        await using var runner = await TestHost.RunAsync(new PathReturner().Wrap());

        using var respose = await runner.GetResponseAsync("//one//two//three//");

        Assert.AreEqual("//one//two//three//", await respose.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyQuery(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(new QueryReturner().Wrap(), engine: engine);

        using var respose = await runner.GetResponseAsync("/?");

        Assert.AreEqual(string.Empty, await respose.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnkeyedQuery(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(new QueryReturner().Wrap(), engine: engine);

        using var respose = await runner.GetResponseAsync("/?query");

        Assert.AreEqual("query=", await respose.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestQueryWithSlashes(TestEngine engine)
    {
        await using var runner = await TestHost.RunAsync(new QueryReturner().Wrap(), engine: engine);

        using var respose = await runner.GetResponseAsync("/?key=/one/two");

        Assert.AreEqual("key=/one/two", await respose.GetContentAsync());
    }

    [TestMethod]
    public async Task TestQueryWithSpaces()
    {
        await using var runner = await TestHost.RunAsync(new QueryReturner().Wrap());

        using var respose = await runner.GetResponseAsync("/?path=/Some+Folder/With%20Subfolders/");

        Assert.AreEqual("path=/Some Folder/With Subfolders/", await respose.GetContentAsync());
    }

    #region Supporting data structures

    private class PathReturner : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request) => request.Respond()
                                                                             .Content(request.Header.Target.AsString())
                                                                             .BuildTask();
    }

    private class QueryReturner : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            // todo: make this logic block reusable
            
            var entries = new List<string>();
            
            for (var i = 0; i < request.Header.Query.Count; i++)
            {
                var entry = request.Header.Query[i];

                var key = HttpUtility.UrlDecode(entry.Key.ToString());
                var value = HttpUtility.UrlDecode(entry.Value.ToString());
                
                entries.Add($"{key}={value}");
            }
            
            var result = string.Join('|', entries);
            
            return request.Respond()
                          .Content(result)
                          .BuildTask();
        }
    }

    #endregion

}
