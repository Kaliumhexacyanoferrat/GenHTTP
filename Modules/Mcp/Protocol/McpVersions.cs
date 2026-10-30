using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Mcp.Protocol;

/// <summary>
/// The revisions of the protocol supported by this implementation.
/// </summary>
/// <remarks>
/// Modern revisions are stateless and carry their version with every request. Legacy
/// revisions negotiate their version with an initialize handshake, which this
/// implementation answers without creating a session.
/// </remarks>
internal static class McpVersions
{

    internal const string Modern = "2026-07-28";

    internal const string LatestLegacy = "2025-11-25";

    /// <summary>
    /// Assumed for requests without a version header, as clients before 2025-06-18 did not send it.
    /// </summary>
    internal const string Unspecified = "2025-03-26";

    internal static readonly string[] Supported = [Modern, LatestLegacy, "2025-06-18", Unspecified];

    private static readonly ByteString[] Encoded = Supported.Select(v => new ByteString(v)).ToArray();

    internal static string? Find(ByteString value)
    {
        for (var i = 0; i < Encoded.Length; i++)
        {
            if (Encoded[i] == value)
            {
                return Supported[i];
            }
        }

        return null;
    }

    internal static bool IsModern(string version) => version == Modern;

    internal static string Negotiate(string? requested)
    {
        if (requested != null)
        {
            for (var i = 1; i < Supported.Length; i++)
            {
                if (Supported[i] == requested)
                {
                    return Supported[i];
                }
            }
        }

        return LatestLegacy;
    }

}
