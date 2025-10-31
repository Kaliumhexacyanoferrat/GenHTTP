using System.Text.Json.Serialization;

namespace GenHTTP.Modules.Mcp.Types;

public sealed class ToolList
{

    [JsonPropertyName("tools")]
    public List<ToolInfo> Tools { get; set; } = new();

}

public sealed class ToolInfo
{

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("inputSchema")]
    public required object InputSchema { get; set; }

    [JsonPropertyName("outputSchema")]
    public required object OutputSchema { get; set; }

}
