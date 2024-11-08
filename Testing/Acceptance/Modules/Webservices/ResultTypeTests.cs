using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    private async Task<TestHost> GetRunnerAsync() => await TestHost.RunAsync(Layout.Create().AddService<TestResource>("t", serializers: Serialization.Default(),
                                                                                                                      injectors: Injection.Default(),
                                                                                                                      formatters: Formatting.Default()));

    #endregion

    #region Tests

    [TestMethod]
    public async Task ControllerMayReturnTask()
    {
        await using var runner = await GetRunnerAsync();

        using var response = await runner.GetResponseAsync("/t/task");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task ControllerMayReturnValueTask()
    {
        await using var runner = await GetRunnerAsync();

        using var response = await runner.GetResponseAsync("/t/value-task");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task ControllerMayReturnGenericTask()
    {
        await using var runner = await GetRunnerAsync();

        using var response = await runner.GetResponseAsync("/t/generic-task");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Task result", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task ControllerMayReturnGenericValueTask()
    {
        await using var runner = await GetRunnerAsync();

        using var response = await runner.GetResponseAsync("/t/generic-value-task");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("ValueTask result", await response.GetContentAsync());
    }

    #endregion

}
