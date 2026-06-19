using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal sealed class InsecureEndPoint : EndPoint
{

    #region Initialization

    internal InsecureEndPoint(IServer server, IPAddress? address, ushort port, bool dualStack)
        : base(server, address, port, dualStack)
    {

    }

    #endregion

    #region Get-/Setters

    public override bool Secure => false;

    #endregion

    #region Functionality

    protected override ValueTask Accept(Socket client)
    {
        return Handle(client, new NetworkStream(client));
    }

    #endregion

}
