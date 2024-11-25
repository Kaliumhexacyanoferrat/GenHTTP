using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class ConcernTests
{

    #region Supporting data structures

    private class DynamicConcern : IConcern
    {

        public IHandler Content { get; }

        public Action Logic { get; }

        public DynamicConcern(IHandler content, Action logic)
        {
            Content = content;
            Logic = logic;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Logic();

            return await Content.HandleAsync(request);
        }

    }

    private class DynamicConcernBuilder : IConcernBuilder
    {
        private readonly Action _Logic;

        public DynamicConcernBuilder(Action logic)
        {
            _Logic = logic;
        }

        public IConcern Build(IHandler content) => new DynamicConcern(content, _Logic);

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConcernOrder(TestEngine engine)
    {
        var i = 0;

        var outer = new DynamicConcernBuilder(() => Assert.AreEqual(0, i++));
        var middle = new DynamicConcernBuilder(() => Assert.AreEqual(1, i++));
        var inner = new DynamicConcernBuilder(() => Assert.AreEqual(2, i++));

        var content = Inline.Create()
                            .Get(() => i)
                            .Add(inner)
                            .Add(middle)
                            .Add(outer);

        await using var host = await TestHost.RunAsync(content, engine: engine);

        var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("3", await response.GetContentAsync());
    }

    #endregion

}
