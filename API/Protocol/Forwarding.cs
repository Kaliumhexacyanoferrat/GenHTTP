using System.Net;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Stores information on how a request has been proxied to the server, as
/// indicated by a single hop of a "Forwarded" header (RFC 7239) or a legacy
/// "X-Forwarded-*" header set.
/// </summary>
/// <param name="For">The client that initiated the request, as seen by the proxy that added this entry</param>
/// <param name="By">The interface of the proxy that added this entry</param>
/// <param name="Host">The host requested by the client, as seen by the proxy that added this entry</param>
/// <param name="Protocol">The protocol used by the client, as seen by the proxy that added this entry</param>
public sealed record Forwarding(IPAddress? For, IPAddress? By, string? Host, ClientProtocol? Protocol);
