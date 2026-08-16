using System.Net;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// One bound endpoint, as the engine sees it. Nothing to dispose: the listener belongs to the
/// reactors, which bind it themselves and tear it down with their rings.
/// </summary>
internal sealed class EndPoint(IPAddress? address, ushort port, bool dualStack, bool secure) : IEndPoint
{
    public IPAddress? Address => address;

    public ushort Port => port;

    public bool DualStack => dualStack;

    public bool Secure => secure;

    public void Dispose() { }
}
