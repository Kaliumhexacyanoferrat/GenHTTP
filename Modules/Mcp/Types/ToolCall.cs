using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenHTTP.Modules.Mcp.Types;

public sealed class ToolCallParams
{

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("arguments")]
    public JsonElement? Arguments { get; init; }

}
