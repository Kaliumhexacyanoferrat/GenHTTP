using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Mcp;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

[TestClass]
public sealed class ToolTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonResult(ServerEngine engine)
    {
        await using var host = await TestServer.RunAsync(engine);

        var response = await McpClient.Modern(host).CallAsync("get_forecast", new JsonObject { ["city"] = "Bern", ["unit"] = "Fahrenheit", ["days"] = 3 });

        var structured = response.Result["structuredContent"]!;

        Assert.AreEqual("Bern", structured["city"]!.GetValue<string>());
        Assert.AreEqual(70.7, structured["temperature"]!.GetValue<double>());
        Assert.IsNull(structured["warning"]);
        Assert.AreEqual("3 day(s)", structured["conditions"]![1]!.GetValue<string>());

        Assert.AreEqual(structured.ToJsonString(), JsonNode.Parse(response.Text)!.ToJsonString());
        Assert.IsFalse(response.IsError);
    }

    [TestMethod]
    public async Task TestDefaultArguments()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("get_forecast", new JsonObject { ["city"] = "Bern" });

        Assert.AreEqual(21.5, response.Result["structuredContent"]!["temperature"]!.GetValue<double>());
    }

    [TestMethod]
    public async Task TestNumbersAsStrings()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("add", new JsonObject { ["a"] = "4", ["b"] = 5 });

        Assert.AreEqual("9", response.Text);
    }

    [TestMethod]
    public async Task TestMissingArguments()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("get_forecast");

        Assert.IsTrue(response.IsError);
        Assert.AreEqual("The argument 'city' is required.", response.Text);
    }

    [TestMethod]
    public async Task TestNullArguments()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync("tools/call", new JsonObject { ["name"] = "list", ["arguments"] = null });

        Assert.IsFalse(response.IsError);
    }

    [TestMethod]
    public async Task TestInvalidArguments()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("add", new JsonObject { ["a"] = "four", ["b"] = 5 });

        Assert.IsTrue(response.IsError);
        AssertX.Contains("The argument 'a' does not match its schema", response.Text);
    }

    [TestMethod]
    public async Task TestArgumentsNeedToBeAnObject()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync("tools/call", new JsonObject { ["name"] = "add", ["arguments"] = new JsonArray() });

        Assert.AreEqual(-32602, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestUnknownTool()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("nope");

        Assert.AreEqual(-32602, response.ErrorCode);
        AssertX.Contains("nope", response.Error["message"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestMissingToolName()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host).RequestAsync("tools/call", new JsonObject());

        Assert.AreEqual(-32602, response.ErrorCode);

        var noParameters = await McpClient.Legacy(host).RequestAsync("tools/call");

        Assert.AreEqual(-32602, noParameters.ErrorCode);
    }

    [TestMethod]
    public async Task TestProviderException()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("fail");

        Assert.IsTrue(response.IsError);
        Assert.AreEqual("The date needs to be in the future", response.Text);
        Assert.IsNull(response.Result["structuredContent"]);
    }

    [TestMethod]
    [DataRow("crash", "Database is down")]
    [DataRow("crash_later", "Disk is full")]
    public async Task TestUnexpectedExceptions(string tool, string message)
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync(tool);

        Assert.IsTrue(response.IsError);
        Assert.AreEqual($"The tool failed unexpectedly: {message}", response.Text);
    }

    [TestMethod]
    public async Task TestMarkdown()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("markdown");

        Assert.AreEqual("# Title", response.Text);
        Assert.IsNull(response.Result["structuredContent"]);
    }

    [TestMethod]
    public async Task TestRequestAccess()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("read_header", configure: r => r.Headers.Add("X-Test", "hello"));

        Assert.AreEqual("hello", response.Text);
        Assert.IsNull(response.Result["structuredContent"]);
    }

    [TestMethod]
    public async Task TestContentBlocks()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("image");

        var content = response.Result["content"]!.AsArray();

        Assert.AreEqual(4, content.Count);

        Assert.AreEqual("A pixel", content[0]!["text"]!.GetValue<string>());

        Assert.AreEqual("image", content[1]!["type"]!.GetValue<string>());
        Assert.AreEqual("AQID", content[1]!["data"]!.GetValue<string>());
        Assert.AreEqual("image/png", content[1]!["mimeType"]!.GetValue<string>());

        Assert.AreEqual("audio", content[2]!["type"]!.GetValue<string>());
        Assert.AreEqual("BAU=", content[2]!["data"]!.GetValue<string>());

        Assert.AreEqual("resource_link", content[3]!["type"]!.GetValue<string>());
        Assert.AreEqual("https://example.com/a.txt", content[3]!["uri"]!.GetValue<string>());
        Assert.AreEqual("a.txt", content[3]!["name"]!.GetValue<string>());
        Assert.AreEqual("A", content[3]!["title"]!.GetValue<string>());
        Assert.AreEqual("A file", content[3]!["description"]!.GetValue<string>());
        Assert.AreEqual("text/plain", content[3]!["mimeType"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestStructuredResult()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host).CallAsync("structured");

        Assert.AreEqual("""{"ok":true,"count":2}""", response.Text);
        Assert.IsTrue(response.Result["structuredContent"]!["ok"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task TestArrayResult()
    {
        await using var host = await TestServer.RunAsync();

        var legacy = await McpClient.Legacy(host).CallAsync("list");

        Assert.AreEqual("""["a","b"]""", legacy.Text);
        Assert.IsNull(legacy.Result["structuredContent"]);

        var modern = await McpClient.Modern(host).CallAsync("list");

        Assert.AreEqual(2, modern.Result["structuredContent"]!.AsArray().Count);
    }

    [TestMethod]
    public async Task TestNonAsciiText()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("echo", new JsonObject { ["text"] = "Nägeli <ti&m>" });

        Assert.AreEqual("Nägeli <ti&m>", response.Text);
    }

    [TestMethod]
    public async Task TestCustomSerialization()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.General);

        await using var host = await TestHost.RunAsync(Tools.Create().Serialization(options).Add<TestTools>());

        var response = await McpClient.Modern(host).CallAsync("get_forecast", new JsonObject { ["city"] = "Bern", ["unit"] = 1 });

        Assert.AreEqual(70.7, response.Result["structuredContent"]!["Temperature"]!.GetValue<double>());
    }

    [TestMethod]
    public async Task TestUnserializableStructuredData()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().Add<BrokenTools>());

        var response = await McpClient.Modern(host).CallAsync("unserializable");

        Assert.IsTrue(response.IsError);
        AssertX.Contains("failed unexpectedly", response.Text);
    }

    [TestMethod]
    public async Task TestUnserializableOutput()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().Add<BrokenTools>());

        var response = await McpClient.Modern(host).CallAsync("unserializable_output");

        Assert.IsTrue(response.IsError);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLargeResult(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(Tools.Create().Add<BigTools>(), engine: engine);

        for (var i = 0; i < 2; i++)
        {
            var response = await McpClient.Modern(host).CallAsync("big");

            Assert.AreEqual(2_000_000, response.Text.Length);
        }
    }

    [TestMethod]
    public async Task TestChecksum()
    {
        ulong? checksum = null;

        var concern = Concern.From(async (request, content) =>
        {
            var response = await content.HandleAsync(request);

            checksum = await response!.Content!.CalculateChecksumAsync();

            return response;
        });

        await using var host = await TestHost.RunAsync(Tools.Create().Add<TestTools>().Add(concern));

        await McpClient.PostAsync(host, """{"jsonrpc":"2.0","id":1,"method":"ping"}""");

        var first = checksum;

        await McpClient.PostAsync(host, """{"jsonrpc":"2.0","id":1,"method":"ping"}""");

        Assert.IsNotNull(first);
        Assert.AreEqual(first, checksum);

        await McpClient.PostAsync(host, """{"jsonrpc":"2.0","id":2,"method":"ping"}""");

        Assert.AreNotEqual(first, checksum);
    }

    [TestMethod]
    public async Task TestErrorResult()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().Add<TestTools>());

        var response = await McpClient.Modern(host).CallAsync("error_result");

        Assert.IsTrue(response.IsError);
        Assert.AreEqual("Not today", response.Text);
    }

    public sealed class BigTools
    {

        [Tool]
        [Summary("Returns a large text")]
        public string Big() => new('x', 2_000_000);

    }

    public sealed class BrokenTools
    {

        [Tool]
        [Summary("Returns data that cannot be serialized")]
        public ToolResult Unserializable() => ToolResult.Json(new Cyclic());

        [Tool]
        [Summary("Returns an object that cannot be serialized")]
        public Cyclic UnserializableOutput() => new();

    }

    public sealed class Cyclic
    {

        public Cyclic Self => this;

    }

}
