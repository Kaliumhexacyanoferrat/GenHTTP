using System.Net.Http.Json;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Mcp;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public sealed class ToolTests
{

    #region Supporting data structures

    public sealed class ServiceTools(AwesomeService awesome)
    {

        [Tool]
        [Summary("Does some work")]
        public string Work(AnotherAwesomeService another, int times = 1) => $"{awesome.DoWork()}/{another.DoWork()}/{times}";

    }

    public sealed class ConstructionCounter
    {
        private int _count;

        public int Count => _count;

        public void Increment() => Interlocked.Increment(ref _count);

    }

    public sealed class CountedTools
    {

        public CountedTools(ConstructionCounter counter)
        {
            counter.Increment();
        }

        [Tool]
        [Summary("Requires a role nobody has")]
        [RequireRole("admin")]
        public string Restricted() => "restricted";

        [Tool]
        [Summary("Open to everybody")]
        public string Open() => "open";

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConstructorAndParameterInjection(ServerEngine engine)
    {
        var tools = DependentTools.Create()
                                  .Add<ServiceTools>();

        await using var host = await DependentHost.RunAsync(tools, engine);

        var list = await SendAsync(host, "tools/list", new JsonObject());

        var properties = list["result"]!["tools"]![0]!["inputSchema"]!["properties"]!.AsObject();

        Assert.HasCount(1, properties);
        Assert.IsTrue(properties.ContainsKey("times"));

        var call = await SendAsync(host, "tools/call", new JsonObject { ["name"] = "work", ["arguments"] = new JsonObject { ["times"] = 2 } });

        Assert.AreEqual("42/24/2", Text(call));
    }

    [TestMethod]
    public async Task TestFunctionInjection()
    {
        var tools = DependentTools.Create()
                                  .Add("work", "Does some work", (AwesomeService service) => service.DoWork());

        await using var host = await DependentHost.RunAsync(tools);

        var call = await SendAsync(host, "tools/call", new JsonObject { ["name"] = "work" });

        Assert.AreEqual("42", Text(call));
    }

    [TestMethod]
    public async Task TestInstancePerCall()
    {
        var counter = new ConstructionCounter();

        var tools = DependentTools.Create()
                                  .Add<CountedTools>();

        await using var host = await DependentHost.RunAsync(tools, configure: services => services.AddSingleton(counter));

        var denied = await SendAsync(host, "tools/call", new JsonObject { ["name"] = "restricted" });

        Assert.IsTrue(denied["result"]!["isError"]!.GetValue<bool>());
        Assert.AreEqual(0, counter.Count);

        Assert.AreEqual("open", Text(await SendAsync(host, "tools/call", new JsonObject { ["name"] = "open" })));
        Assert.AreEqual("open", Text(await SendAsync(host, "tools/call", new JsonObject { ["name"] = "open" })));

        Assert.AreEqual(2, counter.Count);
    }

    private static string Text(JsonNode response) => response["result"]!["content"]![0]!["text"]!.GetValue<string>();

    private static async Task<JsonNode> SendAsync(TestHost host, string method, JsonObject parameters)
    {
        using var request = host.GetRequest("/", HttpMethod.Post);

        request.Content = JsonContent.Create(new JsonObject { ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = method, ["params"] = parameters });

        using var response = await host.GetResponseAsync(request);

        return JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    #endregion

}
