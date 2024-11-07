using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ErrorHandling;

[TestClass]
public sealed class CustomErrorMapperTests
{

    #region Supporting data structures

    public class ErrorLengthMapper : IErrorMapper<Exception>
    {
        public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler) => new(
            request.Respond()
                   .Status(ResponseStatus.NotFound)
                   .Content(Resource.FromString("404").Build())
                   .Build()
        );

        public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error) => new(
            request.Respond()
                   .Status(ResponseStatus.NotFound)
                   .Content(Resource.FromString($"{error.Message.Length}").Build())
                   .Build()
        );
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task Test404Mapped(TestEngine engine)
    {
        var test = Layout.Create()
                         .Add(ErrorHandler.From(new ErrorLengthMapper()));

        using var runner = TestHost.Run(test, engine: engine);

        using var response = await runner.GetResponseAsync("/");
        Assert.AreEqual("404", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestExceptionMapped(TestEngine engine)
    {
        Action thrower = () => throw new Exception("Nope!");

        var test = Layout.Create()
                         .Add(Inline.Create().Get(thrower))
                         .Add(ErrorHandler.From(new ErrorLengthMapper()));

        using var runner = TestHost.Run(test, engine: engine);

        using var response = await runner.GetResponseAsync("/");
        Assert.AreEqual("5", await response.GetContentAsync());
    }

    #endregion

}
