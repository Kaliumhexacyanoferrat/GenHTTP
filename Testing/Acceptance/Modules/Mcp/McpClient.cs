using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

/// <summary>
/// Sends MCP messages the way a modern or a legacy client would.
/// </summary>
internal sealed class McpClient
{
    internal const string ModernVersion = "2026-07-28";

    private static int _nextId;

    private readonly TestHost _host;

    private readonly string? _version;

    private readonly bool _modern;

    private McpClient(TestHost host, string? version, bool modern)
    {
        _host = host;
        _version = version;
        _modern = modern;
    }

    internal static McpClient Modern(TestHost host) => new(host, ModernVersion, true);

    internal static McpClient Legacy(TestHost host, string? version = "2025-06-18") => new(host, version, false);

    internal async Task<McpResponse> RequestAsync(string method, JsonObject? parameters = null, Action<HttpRequestMessage>? configure = null)
    {
        var message = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = Interlocked.Increment(ref _nextId),
            ["method"] = method
        };

        return await SendAsync(message, method, parameters, configure);
    }

    internal async Task<McpResponse> NotifyAsync(string method)
    {
        var message = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method
        };

        return await SendAsync(message, method, null, null);
    }

    internal Task<McpResponse> CallAsync(string tool, JsonObject? arguments = null, Action<HttpRequestMessage>? configure = null)
    {
        var parameters = new JsonObject { ["name"] = tool };

        if (arguments != null)
        {
            parameters["arguments"] = arguments;
        }

        return RequestAsync("tools/call", parameters, configure);
    }

    private async Task<McpResponse> SendAsync(JsonObject message, string method, JsonObject? parameters, Action<HttpRequestMessage>? configure)
    {
        if (_modern)
        {
            parameters ??= [];

            parameters["_meta"] = new JsonObject
            {
                ["io.modelcontextprotocol/protocolVersion"] = _version,
                ["io.modelcontextprotocol/clientInfo"] = new JsonObject { ["name"] = "test-client", ["version"] = "1.2.3" },
                ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject()
            };
        }

        if (parameters != null)
        {
            message["params"] = parameters;
        }

        return await PostAsync(_host, message.ToJsonString(), request =>
        {
            if (_version != null)
            {
                request.Headers.Add("MCP-Protocol-Version", _version);
            }

            if (_modern)
            {
                request.Headers.Add("Mcp-Method", method);

                if (parameters?["name"] is JsonValue name)
                {
                    request.Headers.Add("Mcp-Name", name.GetValue<string>());
                }
            }

            configure?.Invoke(request);
        });
    }

    internal static async Task<McpResponse> PostAsync(TestHost host, string body, Action<HttpRequestMessage>? configure = null)
    {
        using var request = host.GetRequest("/", HttpMethod.Post);

        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        request.Headers.Add("Accept", "application/json, text/event-stream");

        configure?.Invoke(request);

        using var response = await host.GetResponseAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        return new McpResponse(response.StatusCode, content.Length > 0 ? JsonNode.Parse(content) : null, response.Content.Headers.ContentType?.MediaType);
    }

}

internal sealed record McpResponse(HttpStatusCode Status, JsonNode? Body, string? ContentType)
{

    internal JsonObject Result => Body?["result"]?.AsObject() ?? throw new AssertFailedException($"Expected a result, but got: {Body?.ToJsonString()}");

    internal JsonObject Error => Body?["error"]?.AsObject() ?? throw new AssertFailedException($"Expected an error, but got: {Body?.ToJsonString()}");

    internal int ErrorCode => Error["code"]!.GetValue<int>();

    internal string Text => Result["content"]![0]!["text"]!.GetValue<string>();

    internal bool IsError => Result["isError"]!.GetValue<bool>();

}
