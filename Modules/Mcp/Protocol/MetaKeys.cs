namespace GenHTTP.Modules.Mcp.Protocol;

internal static class MetaKeys
{

    internal static ReadOnlySpan<byte> ProtocolVersion => "io.modelcontextprotocol/protocolVersion"u8;

    internal static ReadOnlySpan<byte> ClientCapabilities => "io.modelcontextprotocol/clientCapabilities"u8;

    internal static ReadOnlySpan<byte> ClientInfo => "io.modelcontextprotocol/clientInfo"u8;

    internal static ReadOnlySpan<byte> ServerInfo => "io.modelcontextprotocol/serverInfo"u8;

}
