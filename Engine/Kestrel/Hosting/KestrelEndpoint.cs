using System.Net;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelEndpoint : IEndPoint
{

    #region Get-/Setters

    public IReadOnlyList<IPAddress>? Addresses { get; }

    public ushort Port { get; }

    public bool Secure { get; }

    #endregion

    #region Initialization

    public KestrelEndpoint(IReadOnlyList<IPAddress>? addresses, ushort port, bool secure)
    {
        Addresses = addresses;
        Port = port;
        Secure = secure;
    }

    #endregion

    #region Lifecycle

    public void Dispose() { }

    #endregion

}
