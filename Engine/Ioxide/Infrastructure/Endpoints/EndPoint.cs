using System.Net;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// One bound endpoint, as the engine sees it: where it listens and what it serves. Whether it is
/// secured is the subclass - <see cref="SecureEndPoint"/> or <see cref="InsecureEndPoint"/>.
/// </summary>
/// <remarks>
/// Nothing to dispose: the listener belongs to the reactors, which bind it themselves and tear it
/// down with their rings.
/// </remarks>
internal abstract class EndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols) : IEndPoint
{
    public IPAddress? Address => address;

    public ushort Port => port;

    public bool DualStack => dualStack;

    /// <summary>What this endpoint serves, resolved once from the options and its own binding.</summary>
    public Protocols Protocols => protocols;

    public abstract bool Secure { get; }

    public void Dispose() { }
}
