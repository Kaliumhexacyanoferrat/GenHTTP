using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

#region Supporting data structures

public class TestResource
{

    [ResourceMethod("task")]
    public Task AsyncTask() => Task.CompletedTask;

    [ResourceMethod("value-task")]
    public ValueTask AsyncValueTask() => ValueTask.CompletedTask;

    [ResourceMethod("generic-task")]
    public Task<string> AsyncGenericTask() => Task.FromResult("Task result");

    [ResourceMethod("generic-value-task")]
    public ValueTask<string> AsyncGenericValueTask() => ValueTask.FromResult("ValueTask result");
}

#endregion

[TestClass]
public class ResultTypeTests
{

    #region Helpers

    private async Task<TestHost> GetRunnerAsync(TestEngine engine, ExecutionMode mode) => await TestHost.RunAsync(Layout.Create().AddService<TestResource>("t", serializers: Serialization.Default(),
                                                                                                                      injectors: Injection.Default(),
                                                                                                                      formatters: Formatting.Default(),
                                                                                                                      mode: mode), engine: engine);

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task ControllerMayReturnTask(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/task");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task ControllerMayReturnValueTask(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/value-task");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task ControllerMayReturnGenericTask(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/generic-task");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Task result", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task ControllerMayReturnGenericValueTask(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/generic-value-task");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("ValueTask result", await response.GetContentAsync());
    }

    #endregion

}
