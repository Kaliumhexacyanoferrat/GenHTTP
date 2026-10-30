using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Mcp;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

[TestClass]
public sealed class ProtocolTests
{

    #region Legacy clients

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLegacyHandshake(ServerEngine engine)
    {
        await using var host = await TestServer.RunAsync(engine);

        var client = McpClient.Legacy(host, null);

        var initialize = await client.RequestAsync("initialize", new JsonObject
        {
            ["protocolVersion"] = "2025-06-18",
            ["capabilities"] = new JsonObject(),
            ["clientInfo"] = new JsonObject { ["name"] = "claude-code", ["version"] = "2.0.0" }
        });

        Assert.AreEqual(HttpStatusCode.OK, initialize.Status);
        Assert.AreEqual("application/json", initialize.ContentType);

        var result = initialize.Result;

        Assert.AreEqual("2025-06-18", result["protocolVersion"]!.GetValue<string>());
        Assert.IsNotNull(result["capabilities"]!["tools"]);
        Assert.AreEqual("test-server", result["serverInfo"]!["name"]!.GetValue<string>());
        Assert.AreEqual("Test Server", result["serverInfo"]!["title"]!.GetValue<string>());
        Assert.AreEqual("1.0.0", result["serverInfo"]!["version"]!.GetValue<string>());
        Assert.AreEqual("Call add to add numbers.", result["instructions"]!.GetValue<string>());
        Assert.IsNull(result["resultType"]);

        var initialized = await client.NotifyAsync("notifications/initialized");

        Assert.AreEqual(HttpStatusCode.Accepted, initialized.Status);
        Assert.IsNull(initialized.Body);

        var session = McpClient.Legacy(host, "2025-06-18");

        var tools = await session.RequestAsync("tools/list");

        Assert.AreEqual(TestServer.ToolCount, tools.Result["tools"]!.AsArray().Count);
        Assert.IsNull(tools.Result["ttlMs"]);

        var call = await session.CallAsync("add", new JsonObject { ["a"] = 2, ["b"] = 3 });

        Assert.AreEqual("5", call.Text);
        Assert.IsFalse(call.IsError);
        Assert.IsNull(call.Result["resultType"]);
    }

    [TestMethod]
    [DataRow("2025-11-25", "2025-11-25")]
    [DataRow("2025-03-26", "2025-03-26")]
    [DataRow("2024-11-05", "2025-11-25")]
    [DataRow("2026-07-28", "2025-11-25")]
    public async Task TestVersionNegotiation(string requested, string negotiated)
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host, null).RequestAsync("initialize", new JsonObject { ["protocolVersion"] = requested });

        Assert.AreEqual(negotiated, response.Result["protocolVersion"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestInitializeWithoutParameters()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host, null).RequestAsync("initialize");

        Assert.AreEqual("2025-11-25", response.Result["protocolVersion"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestLegacyWithoutVersionHeader()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host, null).CallAsync("read_header", configure: r => r.Headers.Add("X-Test", "value"));

        Assert.AreEqual("value", response.Text);
    }

    [TestMethod]
    public async Task TestLegacyUnknownMethodIsNotAHttpError()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host).RequestAsync("completion/complete");

        Assert.AreEqual(HttpStatusCode.OK, response.Status);
        Assert.AreEqual(-32601, response.ErrorCode);
    }

    #endregion

    #region Modern clients

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDiscover(ServerEngine engine)
    {
        await using var host = await TestServer.RunAsync(engine);

        var response = await McpClient.Modern(host).RequestAsync("server/discover");

        Assert.AreEqual(HttpStatusCode.OK, response.Status);

        var result = response.Result;

        Assert.AreEqual("complete", result["resultType"]!.GetValue<string>());
        Assert.AreEqual("2026-07-28", result["supportedVersions"]![0]!.GetValue<string>());
        Assert.Contains("2025-11-25", result["supportedVersions"]!.AsArray().Select(v => v!.GetValue<string>()).ToList());
        Assert.IsNotNull(result["capabilities"]!["tools"]);
        Assert.AreEqual("Call add to add numbers.", result["instructions"]!.GetValue<string>());
        Assert.AreEqual(300000, result["ttlMs"]!.GetValue<long>());
        Assert.AreEqual("public", result["cacheScope"]!.GetValue<string>());
        Assert.AreEqual("test-server", result["_meta"]!["io.modelcontextprotocol/serverInfo"]!["name"]!.GetValue<string>());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestModernFlow(ServerEngine engine)
    {
        await using var host = await TestServer.RunAsync(engine);

        var client = McpClient.Modern(host);

        var tools = await client.RequestAsync("tools/list");

        Assert.AreEqual("complete", tools.Result["resultType"]!.GetValue<string>());
        Assert.AreEqual(300000, tools.Result["ttlMs"]!.GetValue<long>());
        Assert.IsTrue(tools.Result["tools"]!.AsArray().Any(t => t!["name"]!.GetValue<string>() == "add"));

        var call = await client.CallAsync("add", new JsonObject { ["a"] = 40, ["b"] = 2 });

        Assert.AreEqual("42", call.Text);
        Assert.IsNull(call.Result["structuredContent"]);
        Assert.AreEqual("complete", call.Result["resultType"]!.GetValue<string>());
        Assert.IsNull(call.Result["ttlMs"]);
        Assert.AreEqual("test-server", call.Result["_meta"]!["io.modelcontextprotocol/serverInfo"]!["name"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestCacheDuration()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().CacheDuration(TimeSpan.FromHours(1)).Add<TestTools>());

        var tools = await McpClient.Modern(host).RequestAsync("tools/list");

        Assert.AreEqual(3600000, tools.Result["ttlMs"]!.GetValue<long>());
    }

    [TestMethod]
    public void TestNegativeCacheDuration()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Tools.Create().CacheDuration(TimeSpan.FromSeconds(-1)));
    }

    [TestMethod]
    public async Task TestModernUnknownMethodIsNotFound()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync("prompts/get", new JsonObject { ["name"] = "x" });

        Assert.AreEqual(HttpStatusCode.NotFound, response.Status);
        Assert.AreEqual(-32601, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestPing()
    {
        await using var host = await TestServer.RunAsync();

        var legacy = await McpClient.Legacy(host).RequestAsync("ping");

        Assert.AreEqual(0, legacy.Result.Count);

        var modern = await McpClient.Modern(host).RequestAsync("ping");

        Assert.AreEqual("complete", modern.Result["resultType"]!.GetValue<string>());
    }

    [TestMethod]
    [DataRow("resources/list", "resources")]
    [DataRow("resources/templates/list", "resourceTemplates")]
    [DataRow("prompts/list", "prompts")]
    public async Task TestEmptyLists(string method, string property)
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync(method);

        Assert.AreEqual(0, response.Result[property]!.AsArray().Count);
        Assert.AreEqual(300000, response.Result["ttlMs"]!.GetValue<long>());
    }

    [TestMethod]
    public async Task TestCursorIsRejected()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync("tools/list", new JsonObject { ["cursor"] = "abc" });

        Assert.AreEqual(-32602, response.ErrorCode);
    }

    #endregion

    #region Request metadata

    [TestMethod]
    public async Task TestUnsupportedVersion()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Legacy(host, "1900-01-01").RequestAsync("tools/list");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32022, response.ErrorCode);
        Assert.AreEqual("1900-01-01", response.Error["data"]!["requested"]!.GetValue<string>());
        Assert.AreEqual("2026-07-28", response.Error["data"]!["supported"]![0]!.GetValue<string>());
        Assert.IsNotNull(response.Body!["id"]);
    }

    [TestMethod]
    public async Task TestMissingVersionHeaderForModernRequest()
    {
        await using var host = await TestServer.RunAsync();

        var body = Message("tools/list", new JsonObject { ["_meta"] = Meta() });

        var response = await McpClient.PostAsync(host, body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32020, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestMissingMeta()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.PostAsync(host, Message("tools/list"), Modern("tools/list"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32602, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestMissingMetaVersion()
    {
        await using var host = await TestServer.RunAsync();

        var meta = Meta();
        meta.Remove("io.modelcontextprotocol/protocolVersion");

        var response = await McpClient.PostAsync(host, Message("tools/list", new JsonObject { ["_meta"] = meta }), Modern("tools/list"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32602, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestMissingCapabilities()
    {
        await using var host = await TestServer.RunAsync();

        var meta = Meta();
        meta.Remove("io.modelcontextprotocol/clientCapabilities");

        var response = await McpClient.PostAsync(host, Message("tools/list", new JsonObject { ["_meta"] = meta }), Modern("tools/list"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32602, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestVersionMismatch()
    {
        await using var host = await TestServer.RunAsync();

        var meta = Meta();
        meta["io.modelcontextprotocol/protocolVersion"] = "2025-11-25";

        var response = await McpClient.PostAsync(host, Message("tools/list", new JsonObject { ["_meta"] = meta }), Modern("tools/list"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32020, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestMissingMethodHeader()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.PostAsync(host, Message("tools/list", new JsonObject { ["_meta"] = Meta() }), Modern(null));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32020, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestMethodMismatch()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.PostAsync(host, Message("tools/list", new JsonObject { ["_meta"] = Meta() }), Modern("server/discover"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32020, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestMissingNameHeader()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.PostAsync(host, Message("tools/call", new JsonObject { ["name"] = "add", ["_meta"] = Meta() }), Modern("tools/call"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32020, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestNameMismatch()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("add", new JsonObject { ["a"] = 1, ["b"] = 2 }, r =>
        {
            r.Headers.Remove("Mcp-Name");
            r.Headers.Add("Mcp-Name", "list");
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(-32020, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestEncodedName()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("add", new JsonObject { ["a"] = 1, ["b"] = 2 }, r =>
        {
            r.Headers.Remove("Mcp-Name");
            r.Headers.Add("Mcp-Name", $"=?base64?{Convert.ToBase64String(Encoding.UTF8.GetBytes("add"))}?=");
        });

        Assert.AreEqual("3", response.Text);
    }

    [TestMethod]
    public async Task TestInvalidEncodedName()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).CallAsync("add", new JsonObject { ["a"] = 1, ["b"] = 2 }, r =>
        {
            r.Headers.Remove("Mcp-Name");
            r.Headers.Add("Mcp-Name", "=?base64?!!!?=");
        });

        Assert.AreEqual(-32020, response.ErrorCode);
    }

    private static string Message(string method, JsonObject? parameters = null)
    {
        var message = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = "req-1", ["method"] = method };

        if (parameters != null)
        {
            message["params"] = parameters;
        }

        return message.ToJsonString();
    }

    private static JsonObject Meta() => new()
    {
        ["io.modelcontextprotocol/protocolVersion"] = McpClient.ModernVersion,
        ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject()
    };

    private static Action<HttpRequestMessage> Modern(string? method) => r =>
    {
        r.Headers.Add("MCP-Protocol-Version", McpClient.ModernVersion);

        if (method != null)
        {
            r.Headers.Add("Mcp-Method", method);
        }
    };

    #endregion

    #region Transport

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetIsNotAllowed(ServerEngine engine)
    {
        await using var host = await TestServer.RunAsync(engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.MethodNotAllowed);

        Assert.AreEqual("POST", string.Join(",", response.Content.Headers.Allow));
    }

    [TestMethod]
    public async Task TestForeignOriginIsRejected()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.Modern(host).RequestAsync("tools/list", configure: r => r.Headers.Add("Origin", "https://evil.example"));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.Status);
        Assert.IsNull(response.Body!["id"]);
    }

    [TestMethod]
    public async Task TestAllowedOrigin()
    {
        await using var host = await TestHost.RunAsync(Tools.Create().AllowOrigin("https://inspector.example").Add<TestTools>());

        var allowed = await McpClient.Modern(host).RequestAsync("tools/list", configure: r => r.Headers.Add("Origin", "https://INSPECTOR.example:443"));

        Assert.AreEqual(HttpStatusCode.OK, allowed.Status);

        var invalid = await McpClient.Modern(host).RequestAsync("tools/list", configure: r => r.Headers.Add("Origin", "null"));

        Assert.AreEqual(HttpStatusCode.Forbidden, invalid.Status);
    }

    [TestMethod]
    public void TestInvalidAllowedOrigin()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Tools.Create().AllowOrigin("not an origin").Build());
    }

    [TestMethod]
    [DataRow("{ not json", -32700)]
    [DataRow("[]", -32600)]
    [DataRow("""{"jsonrpc":"2.0","id":{},"method":"ping"}""", -32600)]
    [DataRow("""{"jsonrpc":"1.0","id":1,"method":"ping"}""", -32600)]
    [DataRow("""{"id":1,"method":"ping"}""", -32600)]
    [DataRow("""{"jsonrpc":"2.0","id":1,"result":{}}""", -32600)]
    [DataRow("""{"jsonrpc":"2.0","id":1,"method":"ping","params":[]}""", -32600)]
    public async Task TestMalformedMessages(string body, int code)
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.PostAsync(host, body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(code, response.ErrorCode);
    }

    [TestMethod]
    public async Task TestEmptyBody()
    {
        await using var host = await TestServer.RunAsync();

        using var request = host.GetRequest("/", HttpMethod.Post);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task TestStringId()
    {
        await using var host = await TestServer.RunAsync();

        var response = await McpClient.PostAsync(host, """{"jsonrpc":"2.0","id":"abc","method":"ping"}""");

        Assert.AreEqual("abc", response.Body!["id"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestConcern()
    {
        var concern = Concern.From((request, content) =>
        {
            if (request.Header.Headers.GetEntry("Authorization") != "Bearer secret")
            {
                return new(request.Respond().Status(ResponseStatus.Unauthorized).Build());
            }

            return content.HandleAsync(request);
        });

        var mcp = Tools.Create()
                       .Add<TestTools>()
                       .Add(concern);

        await using var host = await TestHost.RunAsync(mcp);

        var denied = await McpClient.Modern(host).RequestAsync("tools/list");

        Assert.AreEqual(HttpStatusCode.Unauthorized, denied.Status);

        var allowed = await McpClient.Modern(host).RequestAsync("tools/list", configure: r => r.Headers.Add("Authorization", "Bearer secret"));

        Assert.AreEqual(HttpStatusCode.OK, allowed.Status);
    }

    #endregion

}
