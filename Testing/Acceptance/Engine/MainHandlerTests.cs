using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class MainHandlerTests
{

    #region Supporting data structures

    public class PreparationHandler : IHandler
    {
        private bool _Prepared;

        public ValueTask PrepareAsync()
        {
            _Prepared = true;
            return new();
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = request.Respond()
                                  .Content(new StringContent(_Prepared ? "prepared" : "not prepared"))
                                  .Build();

            return new(response);
        }

    }

    #endregion

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHandlerPreparation(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(new PreparationHandler(), engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("prepared", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLayoutHandlerPreparation(TestEngine engine)
    {
        var app = Layout.Create().Add("sub", new PreparationHandler());

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/sub/");

        Assert.AreEqual("prepared", await response.GetContentAsync());
    }

}
