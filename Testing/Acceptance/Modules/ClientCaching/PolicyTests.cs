using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ClientCaching;

[TestClass]
public sealed class PolicyTests
{

    [TestMethod]
    public async Task TestExpireHeaderSet()
    {
        var content = Content.From(Resource.FromString("Content"))
                             .Add(ClientCache.Policy().Duration(1));

        using var runner = TestHost.Run(content);

        using var response = await runner.GetResponseAsync();

        Assert.IsNotNull(response.GetContentHeader("Expires"));
    }

    [TestMethod]
    public async Task TestExpireHeaderNotSetForOtherMethods()
    {
        var content = Content.From(Resource.FromString("Content"))
                             .Add(ClientCache.Policy().Duration(1));

        using var runner = TestHost.Run(content);

        var request = runner.GetRequest();
        request.Method = HttpMethod.Head;

        using var response = await runner.GetResponseAsync(request);

        AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
    }

    [TestMethod]
    public async Task TestExpireHeaderNotSetForOtherStatus()
    {
        var content = Layout.Create()
                            .Add(ClientCache.Policy().Duration(1));

        using var runner = TestHost.Run(content);

        using var response = await runner.GetResponseAsync();

        AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
    }

    [TestMethod]
    public async Task TestPredicate()
    {
        var content = Content.From(Resource.FromString("Content"))
                             .Add(ClientCache.Policy().Duration(1).Predicate((_, r) => r.ContentType?.RawType != "text/plain"));

        using var runner = TestHost.Run(content);

        using var response = await runner.GetResponseAsync();

        AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
    }
}
