using System.Text.Json.Nodes;

using GenHTTP.Modules.Mcp;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

[TestClass]
public sealed class DefinitionTests
{

    [TestMethod]
    public async Task TestGeneratedInputSchema()
    {
        var add = await GetToolAsync("add");

        Assert.AreEqual("Adds two integers", add["description"]!.GetValue<string>());
        Assert.IsNull(add["title"]);
        Assert.IsNull(add["annotations"]);

        var schema = add["inputSchema"]!;

        Assert.AreEqual("object", schema["type"]!.GetValue<string>());
        Assert.AreEqual("integer", schema["properties"]!["a"]!["type"]!.GetValue<string>());
        Assert.AreEqual("The first summand", schema["properties"]!["a"]!["description"]!.GetValue<string>());
        Assert.AreEqual("The second summand", schema["properties"]!["b"]!["description"]!.GetValue<string>());
        Assert.AreEqual("""["a","b"]""", schema["required"]!.ToJsonString());

        Assert.IsNull(add["outputSchema"]);
    }

    [TestMethod]
    public async Task TestOptionalInput()
    {
        var schema = (await GetToolAsync("get_forecast"))["inputSchema"]!;

        Assert.AreEqual("""["city"]""", schema["required"]!.ToJsonString());
        Assert.AreEqual("The city to forecast", schema["properties"]!["city"]!["description"]!.GetValue<string>());
        Assert.AreEqual("""["Celsius","Fahrenheit"]""", schema["properties"]!["unit"]!["enum"]!.ToJsonString());
        Assert.AreEqual("Celsius", schema["properties"]!["unit"]!["default"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestOutputSchema()
    {
        var output = (await GetToolAsync("get_forecast"))["outputSchema"]!;

        Assert.AreEqual("object", output["type"]!.GetValue<string>());
        Assert.AreEqual("""["city","temperature","conditions"]""", output["required"]!.ToJsonString());
        Assert.IsNotNull(output["properties"]!["warning"]);
        Assert.IsNotNull(output["properties"]!["unit"]);

        var alert = output["properties"]!["alerts"]!["items"]!;

        Assert.AreEqual("""["kind"]""", alert["required"]!.ToJsonString());
    }

    [TestMethod]
    public async Task TestNullableValuesAreOmitted()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("get_forecast", new JsonObject { ["city"] = "Bern" });

        var alert = response.Result["structuredContent"]!["alerts"]![0]!.AsObject();

        Assert.AreEqual("wind", alert["kind"]!.GetValue<string>());
        Assert.IsFalse(alert.ContainsKey("note"));
    }

    [TestMethod]
    public async Task TestNoOutputSchemaForNonObjects()
    {
        Assert.IsNull((await GetToolAsync("list"))["outputSchema"]);
        Assert.IsNull((await GetToolAsync("image"))["outputSchema"]);
        Assert.IsNull((await GetToolAsync("read_header"))["outputSchema"]);
    }

    [TestMethod]
    public async Task TestAnnotations()
    {
        var forecast = await GetToolAsync("get_forecast");

        Assert.AreEqual("Weather forecast", forecast["title"]!.GetValue<string>());

        var annotations = forecast["annotations"]!;

        Assert.AreEqual("Weather forecast", annotations["title"]!.GetValue<string>());
        Assert.IsTrue(annotations["readOnlyHint"]!.GetValue<bool>());
        Assert.IsFalse(annotations["destructiveHint"]!.GetValue<bool>());
        Assert.IsTrue(annotations["idempotentHint"]!.GetValue<bool>());
        Assert.IsTrue(annotations["openWorldHint"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task TestFunctionHints()
    {
        var tools = Tools.Create()
                         .Add("read", "Reads", [Tool(ReadOnly = true, Destructive = false, Idempotent = true)] () => 1)
                         .Add("create", "Creates", [Tool(ReadOnly = false, Destructive = false, Idempotent = false)] () => 1)
                         .Add("replace", "Replaces", [Tool(ReadOnly = false, Destructive = true, Idempotent = true)] () => 1)
                         .Add("plain", "Has no hints", () => 1);

        await using var host = await TestHost.RunAsync(tools);

        var response = await McpClient.Modern(host).RequestAsync("tools/list");

        var hints = response.Result["tools"]!.AsArray().ToDictionary(t => t!["name"]!.GetValue<string>(), t => t!["annotations"]?.ToJsonString());

        Assert.AreEqual("""{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":true}""", hints["read"]);
        Assert.AreEqual("""{"readOnlyHint":false,"destructiveHint":false,"idempotentHint":false}""", hints["create"]);
        Assert.AreEqual("""{"readOnlyHint":false,"destructiveHint":true,"idempotentHint":true}""", hints["replace"]);
        Assert.IsNull(hints["plain"]);
    }

    [TestMethod]
    public void TestFunctionNameViaAttributeIsRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add("a", "A tool", [Tool("b")] () => 1));
    }

    [TestMethod]
    public async Task TestEmptyInput()
    {
        var schema = (await GetToolAsync("list"))["inputSchema"]!;

        Assert.AreEqual("object", schema["type"]!.GetValue<string>());
        Assert.IsFalse(schema["additionalProperties"]!.GetValue<bool>());

        Assert.IsNull((await GetToolAsync("add"))["inputSchema"]!["additionalProperties"]);
    }

    [TestMethod]
    public async Task TestOrderIsStable()
    {
        await using var host = await TestServer.RunAsync();

        var first = await McpClient.Modern(host).RequestAsync("tools/list");
        var second = await McpClient.Modern(host).RequestAsync("tools/list");

        Assert.AreEqual(first.Result["tools"]!.ToJsonString(), second.Result["tools"]!.ToJsonString());
    }

    [TestMethod]
    public void TestDuplicateNamesAreRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add("a", "A", () => 1).Add("a", "A", () => 2).Build());
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add<TestTools>().Add("add", "Adds", () => 1).Build());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("with space")]
    [DataRow("umläut")]
    [DataRow("a/b")]
    public void TestInvalidNamesAreRejected(string name)
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add(name, "A tool", () => 1));
    }

    [TestMethod]
    public void TestLongNamesAreRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add(new string('a', 129), "A tool", () => 1));
    }

    [TestMethod]
    public async Task TestValidNames()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().Add("admin.tools-list_V2", "A tool", () => "valid"));

        var response = await McpClient.Modern(host).CallAsync("admin.tools-list_V2");

        Assert.AreEqual("valid", response.Text);
    }

    [TestMethod]
    public async Task TestDefaultServerInfo()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().Add<TestTools>());

        var response = await McpClient.Legacy(host, null).RequestAsync("initialize");

        var info = response.Result["serverInfo"]!;

        Assert.IsFalse(string.IsNullOrEmpty(info["name"]!.GetValue<string>()));
        Assert.IsFalse(string.IsNullOrEmpty(info["version"]!.GetValue<string>()));
        Assert.IsNull(info["title"]);
        Assert.IsNull(response.Result["instructions"]);
    }

    private static async Task<JsonNode> GetToolAsync(string name)
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync("tools/list");

        return response.Result["tools"]!.AsArray().Single(t => t!["name"]!.GetValue<string>() == name)!;
    }

}
