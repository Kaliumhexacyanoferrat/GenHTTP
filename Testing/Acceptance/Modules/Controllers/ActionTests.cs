using System.Net;
using System.Net.Http.Headers;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class ActionTests
{

    #region Helpers

    private async Task<TestHost> GetRunnerAsync(TestEngine engine, ExecutionMode mode) => await TestHost.RunAsync(Layout.Create().AddController<TestController>("t", mode: mode), engine: engine);

    #endregion

    #region Supporting data structures

    public sealed class Model
    {

        public string? Field { get; set; }
    }

    public sealed class TestController
    {

        public string Index() => "Hello World!";

        public IHandlerBuilder Action(int? query) => Content.From(Resource.FromString(query?.ToString() ?? "Action"));

        [ControllerAction(RequestMethod.Put)]
        public IHandlerBuilder Action(int? value1, string value2) => Content.From(Resource.FromString((value1?.ToString() ?? "Action") + $" {value2}"));

        public IHandlerBuilder SimpleAction([FromPath] int id) => Content.From(Resource.FromString(id.ToString()));

        public IHandlerBuilder ComplexAction(int three, [FromPath] int one, [FromPath] int two) => Content.From(Resource.FromString((one + two + three).ToString()));

        [ControllerAction(RequestMethod.Post)]
        public IHandlerBuilder Action(Model data) => Content.From(Resource.FromString(data.Field ?? "no content"));

        public IHandlerBuilder HypenCAsing99() => Content.From(Resource.FromString("OK"));

        public void Void() { }
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestIndex(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestAction(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/action/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Action", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithQuery(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/action/?query=0815");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("815", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithQueryFromBody(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        var dict = new Dictionary<string, string>
        {
            {
                "value2", "test"
            }
        };

        var request = runner.GetRequest("/t/action/");

        request.Method = HttpMethod.Put;
        request.Content = new FormUrlEncodedContent(dict);

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Action test", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithBody(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        var request = runner.GetRequest("/t/action/");

        request.Method = HttpMethod.Post;

        request.Content = new StringContent("{ \"field\": \"FieldData\" }", null, "application/json");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("FieldData", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithParameter(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/simple-action/4711/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("4711", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithBadParameter(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/simple-action/string/");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithMixedParameters(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/complex-action/1/2/?three=3");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("6", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestActionWithNoResult(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/void/");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNonExistingAction(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/nope/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestHypenCasing(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await GetRunnerAsync(engine, mode);

        using var response = await runner.GetResponseAsync("/t/hypen-casing-99/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("OK", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestIndexController(TestEngine engine, ExecutionMode mode)
    {
        await using var runner = await TestHost.RunAsync(Layout.Create().IndexController<TestController>(mode: mode), engine: engine);

        using var response = await runner.GetResponseAsync("/simple-action/4711/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("4711", await response.GetContentAsync());
    }

    #endregion

}
