using System.Text.Json;

namespace GenHTTP.Modules.Mcp.Protocol;

/// <summary>
/// Writes the JSON-RPC messages sent by the server, adding the fields
/// required by the protocol version of the client.
/// </summary>
internal sealed class RpcMessages
{
    private readonly byte[] _serverInfo;

    private readonly long _cacheDuration;

    #region Get-/Setters

    internal ReadOnlySpan<byte> ServerInfo => _serverInfo;

    #endregion

    #region Initialization

    internal RpcMessages(string name, string version, string? title, TimeSpan cacheDuration)
    {
        _cacheDuration = (long)cacheDuration.TotalMilliseconds;

        using var buffer = JsonBuffer.Rent();

        var w = buffer.Writer;

        w.WriteStartObject();
        w.WriteString("name"u8, name);

        if (title != null)
        {
            w.WriteString("title"u8, title);
        }

        w.WriteString("version"u8, version);
        w.WriteEndObject();

        _serverInfo = buffer.ToArray();
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Starts a result message, to be completed via <see cref="EndResult" />.
    /// </summary>
    internal static JsonBuffer BeginResult(JsonElement id)
    {
        var buffer = JsonBuffer.Rent();
        var w = buffer.Writer;

        w.WriteStartObject();
        w.WriteString("jsonrpc"u8, "2.0"u8);

        w.WritePropertyName("id"u8);
        id.WriteTo(w);

        w.WriteStartObject("result"u8);

        return buffer;
    }

    internal byte[] EndResult(JsonBuffer buffer, bool modern, bool cacheable)
    {
        var w = buffer.Writer;

        if (modern)
        {
            w.WriteString("resultType"u8, "complete"u8);

            if (cacheable)
            {
                w.WriteNumber("ttlMs"u8, _cacheDuration);
                w.WriteString("cacheScope"u8, "public"u8);
            }

            w.WriteStartObject("_meta"u8);
            w.WritePropertyName(MetaKeys.ServerInfo);
            w.WriteRawValue(_serverInfo, skipInputValidation: true);
            w.WriteEndObject();
        }

        w.WriteEndObject();
        w.WriteEndObject();

        var content = buffer.ToArray();

        buffer.Dispose();

        return content;
    }

    internal static byte[] Error(JsonElement? id, int code, string message)
    {
        using var buffer = BeginError(id, code, message);

        buffer.Writer.WriteEndObject();
        buffer.Writer.WriteEndObject();

        return buffer.ToArray();
    }

    internal static byte[] UnsupportedVersion(JsonElement id, string requested)
    {
        using var buffer = BeginError(id, RpcErrors.UnsupportedProtocolVersion, "Unsupported protocol version");

        var w = buffer.Writer;

        w.WriteStartObject("data"u8);
        WriteSupportedVersions(w, "supported"u8);
        w.WriteString("requested"u8, requested);
        w.WriteEndObject();

        w.WriteEndObject();
        w.WriteEndObject();

        return buffer.ToArray();
    }

    internal static void WriteSupportedVersions(Utf8JsonWriter writer, ReadOnlySpan<byte> property)
    {
        writer.WriteStartArray(property);

        foreach (var supported in McpVersions.Supported)
        {
            writer.WriteStringValue(supported);
        }

        writer.WriteEndArray();
    }

    private static JsonBuffer BeginError(JsonElement? id, int code, string message)
    {
        var buffer = JsonBuffer.Rent();
        var w = buffer.Writer;

        w.WriteStartObject();
        w.WriteString("jsonrpc"u8, "2.0"u8);

        if (id != null)
        {
            w.WritePropertyName("id"u8);
            id.Value.WriteTo(w);
        }

        w.WriteStartObject("error"u8);
        w.WriteNumber("code"u8, code);
        w.WriteString("message"u8, message);

        return buffer;
    }

    #endregion

}
