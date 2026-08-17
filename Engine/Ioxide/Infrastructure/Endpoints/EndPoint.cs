using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// One bound endpoint, as the engine sees it: where it listens, and what secures it.
/// </summary>
/// <remarks>
/// <see cref="Security"/> carries the binding's certificate provider, protocols and client
/// certificate validator, and <see cref="Protocols"/> what the port serves, so an endpoint answers
/// for itself rather than the server keeping second tables keyed by port. Nothing to dispose: the
/// listener belongs to the reactors, which bind it themselves and tear it down with their rings.
/// </remarks>
internal sealed class EndPoint(IPAddress? address, ushort port, bool dualStack, SecurityConfiguration? security, Protocols protocols) : IEndPoint
{
    public IPAddress? Address => address;

    public ushort Port => port;

    public bool DualStack => dualStack;

    /// <summary>How this endpoint is secured, or null for a plaintext one.</summary>
    public SecurityConfiguration? Security => security;

    public bool Secure => security is not null;

    /// <summary>What this endpoint serves, resolved once from the options and its own binding.</summary>
    public Protocols Protocols => protocols;

    public void Dispose() { }
}
