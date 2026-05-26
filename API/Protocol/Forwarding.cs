using System.Net;

namespace GenHTTP.Api.Protocol;

// todo: re-visit

/// <summary>
/// Stores information how a request has been proxied
/// to the server.
/// </summary>
public record Forwarding(IPAddress? For, string? Host, ClientProtocol? Protocol);
