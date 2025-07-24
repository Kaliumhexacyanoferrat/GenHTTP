using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public class BasicTests
{

    #region Supporting data structures

    public class MyConcern(AwesomeService service) : IDependentConcern
    {

        public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
        {
            var response = await content.HandleAsync(request);

            if (response != null)
            {
                response.Headers.Add("X-Custom", service.DoWork());
            }

            return response;
        }
    }

    public class MyHandler(AwesomeService service) : IDependentHandler
    {

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = request.Respond()
                          .Content(service.DoWork())
                          .Build();

            return ValueTask.FromResult<IResponse?>(response);
        }

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDependentConcern(TestEngine engine)
    {
        var concern = Dependent.Concern<MyConcern>();

        var app = Inline.Create()
                        .Get(() => "Yay")
                        .Add(concern);

        await using var host = await DependentHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        Assert.AreEqual("42", response.GetHeader("X-Custom"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDependentHandler(TestEngine engine)
    {
        var handler = Dependent.Handler<MyHandler>();

        await using var host = await DependentHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    #endregion;

}
