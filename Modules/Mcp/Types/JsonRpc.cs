using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenHTTP.Modules.Mcp.Types;

public abstract class JsonRpcBase
{

    [JsonPropertyName("jsonrpc")]
    public string Version { get; init; } = "2.0";

}

public sealed class JsonRpcRequest : JsonRpcBase
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    public JsonElement? Params { get; init; }
}

public sealed class JsonRpcResponse<TResult> : JsonRpcBase
{

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("result")]
    public TResult? Result { get; init; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; init; }

    [JsonIgnore]
    public bool IsError => Error != null;

}

public sealed class JsonRpcError
{

    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("data")]
    public object? Data { get; init; }

}
