using System.Net;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class EndPoint: IEndPoint
{

    #region Get-/Setters

    public IPAddress? Address { get; set; }

    public bool DualStack { get; set; }

    public ushort Port { get; set; }

    public bool Secure { get; set; }

    #endregion

    #region Initialization

    public EndPoint()
    {
        DualStack = true;
        Port = 8080;
        Secure = false;
    }

    #endregion

    #region Functionality

    public void Dispose()
    {
        // nop
    }

    #endregion

}
