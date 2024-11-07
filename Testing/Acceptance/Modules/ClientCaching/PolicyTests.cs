using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ClientCaching;

[TestClass]
public sealed class PolicyTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestExpireHeaderSet(TestEngine engine)
    {
        var content = Content.From(Resource.FromString("Content"))
                             .Add(ClientCache.Policy().Duration(1));

        using var runner = TestHost.Run(content, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.IsNotNull(response.GetContentHeader("Expires"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestExpireHeaderNotSetForOtherMethods(TestEngine engine)
    {
        var content = Content.From(Resource.FromString("Content"))
                             .Add(ClientCache.Policy().Duration(1));

        using var runner = TestHost.Run(content, engine: engine);

        var request = runner.GetRequest();
        request.Method = HttpMethod.Head;

        using var response = await runner.GetResponseAsync(request);

        AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestExpireHeaderNotSetForOtherStatus(TestEngine engine)
    {
        var content = Layout.Create()
                            .Add(ClientCache.Policy().Duration(1));

        using var runner = TestHost.Run(content, engine: engine);

        using var response = await runner.GetResponseAsync();

        AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPredicate(TestEngine engine)
    {
        var content = Content.From(Resource.FromString("Content"))
                             .Add(ClientCache.Policy().Duration(1).Predicate((_, r) => r.ContentType?.RawType != "text/plain"));

        using var runner = TestHost.Run(content, engine: engine);

        using var response = await runner.GetResponseAsync();

        AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
    }
}
