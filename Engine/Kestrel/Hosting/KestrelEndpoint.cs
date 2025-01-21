using System.Net;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelEndpoint : IEndPoint
{

    #region Get-/Setters

    public IPAddress? Address { get; }

    public ushort Port { get; }

    public bool Secure { get; }

    #endregion

    #region Initialization

    public KestrelEndpoint(IPAddress? address, ushort port, bool secure)
    {
        Address = address;
        Port = port;
        Secure = secure;
    }

    #endregion

    #region Lifecycle

    public void Dispose() { }

    #endregion

}
