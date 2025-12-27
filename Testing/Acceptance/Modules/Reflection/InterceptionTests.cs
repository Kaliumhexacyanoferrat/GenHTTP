using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public class InterceptionTests
{

    #region Supporting data structures

    [AttributeUsage(AttributeTargets.Method)]
    public class MyAttribute(string command) : InterceptWithAttribute<MyInterceptor>
    {

        public string Command => command;

    }

    public class MyInterceptor : IOperationInterceptor
    {

        public string? Command { get; private set; }

        public void Configure(object attribute)
        {
            if (attribute is MyAttribute my)
            {
                Command = my.Command;
            }
        }

        public ValueTask<InterceptionResult?> InterceptAsync(IRequest request, Operation operation, IReadOnlyDictionary<string, object?> arguments)
        {
            if (Command == "intercept")
            {
                var result = new InterceptionResult("Nah");
                result.Status(ResponseStatus.Forbidden);

                return new(result);
            }

            if (Command == "throw")
            {
                throw new ProviderException(ResponseStatus.Forbidden, "Nah");
            }

            return default;
        }

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestInterception(TestEngine engine, ExecutionMode mode)
    {
        var app = Inline.Create().Get([My("intercept")] () => 42).ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(app);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);

        Assert.AreEqual("Nah", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestPassThrough(TestEngine engine, ExecutionMode mode)
    {
        var app = Inline.Create().Get([My("pass")] () => 42).ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(app);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestException(TestEngine engine, ExecutionMode mode)
    {
        var app = Inline.Create().Get([My("throw")] () => 42).ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(app);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);

        AssertX.Contains("Nah", await response.GetContentAsync());
    }

    #endregion

}
