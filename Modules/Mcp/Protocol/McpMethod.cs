using System.Text.Json;

namespace GenHTTP.Modules.Mcp.Protocol;

internal enum McpMethod
{
    Unknown,
    Initialize,
    Discover,
    Ping,
    ListTools,
    CallTool,
    ListResources,
    ListResourceTemplates,
    ListPrompts
}

internal static class McpMethods
{
    private static readonly (byte[] Encoded, string Name, McpMethod Method)[] Known =
    [
        ("tools/call"u8.ToArray(), "tools/call", McpMethod.CallTool),
        ("tools/list"u8.ToArray(), "tools/list", McpMethod.ListTools),
        ("server/discover"u8.ToArray(), "server/discover", McpMethod.Discover),
        ("initialize"u8.ToArray(), "initialize", McpMethod.Initialize),
        ("ping"u8.ToArray(), "ping", McpMethod.Ping),
        ("resources/list"u8.ToArray(), "resources/list", McpMethod.ListResources),
        ("resources/templates/list"u8.ToArray(), "resources/templates/list", McpMethod.ListResourceTemplates),
        ("prompts/list"u8.ToArray(), "prompts/list", McpMethod.ListPrompts)
    ];

    internal static McpMethod Identify(JsonElement method, out string? name)
    {
        foreach (var known in Known)
        {
            if (method.ValueEquals(known.Encoded))
            {
                name = known.Name;
                return known.Method;
            }
        }

        name = null;
        return McpMethod.Unknown;
    }

}
