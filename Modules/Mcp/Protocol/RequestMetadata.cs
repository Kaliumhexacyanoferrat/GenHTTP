using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Mcp.Protocol;

/// <summary>
/// Validates the metadata modern clients send with every request, both
/// in the body and mirrored into headers for intermediaries.
/// </summary>
internal static class RequestMetadata
{
    private static readonly ByteString MethodHeader = new("Mcp-Method");

    private static readonly ByteString NameHeader = new("Mcp-Name");

    internal static string? Validate(IRequest request, string version, McpMethod method, JsonElement methodElement, JsonElement parameters, out int code)
    {
        code = RpcErrors.InvalidParams;

        if (!TryGetMeta(parameters, out var meta))
        {
            return "The request does not carry the required _meta object";
        }

        if (!meta.TryGetProperty(MetaKeys.ProtocolVersion, out var metaVersion) || metaVersion.ValueKind != JsonValueKind.String)
        {
            return "The _meta field 'io.modelcontextprotocol/protocolVersion' is required";
        }

        if (!meta.TryGetProperty(MetaKeys.ClientCapabilities, out var capabilities) || capabilities.ValueKind != JsonValueKind.Object)
        {
            return "The _meta field 'io.modelcontextprotocol/clientCapabilities' is required";
        }

        code = RpcErrors.HeaderMismatch;

        if (!metaVersion.ValueEquals(version))
        {
            return "The MCP-Protocol-Version header does not match the protocol version given in _meta";
        }

        var headers = request.Header.Headers;

        var methodHeader = headers.GetEntry(MethodHeader);

        if (methodHeader == null)
        {
            return "The Mcp-Method header is required";
        }

        if (!methodElement.ValueEquals(methodHeader.Value.Bytes.Span))
        {
            return "The Mcp-Method header does not match the method of the request";
        }

        if (method == McpMethod.CallTool && parameters.TryGetProperty("name"u8, out var name) && name.ValueKind == JsonValueKind.String)
        {
            var nameHeader = headers.GetEntry(NameHeader);

            if (nameHeader == null)
            {
                return "The Mcp-Name header is required";
            }

            if (!TryDecodeHeader(nameHeader.Value.Bytes.Span, out var decoded) || !name.ValueEquals(decoded))
            {
                return "The Mcp-Name header does not match the name of the tool";
            }
        }

        return null;
    }

    internal static bool TryGetMeta(JsonElement parameters, out JsonElement meta)
    {
        if (parameters.ValueKind == JsonValueKind.Object && parameters.TryGetProperty("_meta"u8, out meta) && meta.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        meta = default;
        return false;
    }

    private static bool TryDecodeHeader(ReadOnlySpan<byte> value, out ReadOnlySpan<byte> decoded)
    {
        var prefix = "=?base64?"u8;
        var suffix = "?="u8;

        if (value.Length < prefix.Length + suffix.Length || !value.StartsWith(prefix) || !value.EndsWith(suffix))
        {
            decoded = value;
            return true;
        }

        var encoded = value[prefix.Length..^suffix.Length];

        var buffer = new byte[Base64.GetMaxDecodedFromUtf8Length(encoded.Length)];

        if (Base64.DecodeFromUtf8(encoded, buffer, out _, out var written) != OperationStatus.Done)
        {
            decoded = default;
            return false;
        }

        decoded = buffer.AsSpan(0, written);
        return true;
    }

}
