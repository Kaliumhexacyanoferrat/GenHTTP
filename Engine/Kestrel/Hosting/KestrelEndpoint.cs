using System.Net;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelEndpoint : IEndPoint
{

    #region Get-/Setters

    public IPAddress? Address { get; }

    public bool DualStack { get; }

    public ushort Port { get; }

    public bool Secure { get; }

    #endregion

    #region Initialization

    public KestrelEndpoint(IPAddress? address, ushort port, bool dualStack, bool secure)
    {
        Address = address;
        Port = port;
        DualStack = dualStack;
        Secure = secure;
    }

    #endregion

    #region Lifecycle

    public void Dispose() { }

    #endregion

}
