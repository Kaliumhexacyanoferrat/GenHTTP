using System.Net;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>One bound address and port, and what it serves.</summary>
internal abstract class EndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols) : IEndPoint
{
    public IPAddress? Address => address;

    public ushort Port => port;

    public bool DualStack => dualStack;

    public Protocols Protocols => protocols;

    public abstract bool Secure { get; }

    // Nothing to release: the reactor owns the listener, not the endpoint.
    public void Dispose() { }
}
