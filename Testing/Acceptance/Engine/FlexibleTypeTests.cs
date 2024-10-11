using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class FlexibleTypeTests
{

    /// <summary>
    ///     As a developer I would like to use status codes and content types
    ///     not supported by the server.
    /// </summary>
    [TestMethod]
    public async Task TestFlexibleStatus()
    {
        var content = Layout.Create().Index(new Provider().Wrap());

        using var runner = TestHost.Run(content);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual(256, (int)response.StatusCode);
        Assert.AreEqual("Custom Status", response.ReasonPhrase);

        Assert.AreEqual("application/x-custom", response.GetContentHeader("Content-Type"));
    }

    private class Provider : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IHandler Parent => throw new NotImplementedException();

        public ValueTask<IResponse?> HandleAsync(IRequest request) => request.Respond()
                                                                             .Content("Hello World!")
                                                                             .Type("application/x-custom")
                                                                             .Status(256, "Custom Status")
                                                                             .BuildTask();
    }
}
