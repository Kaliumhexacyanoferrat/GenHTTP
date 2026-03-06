using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal sealed class InsecureEndPoint : EndPoint
{

    #region Initialization

    internal InsecureEndPoint(IServer server, IPAddress? address, ushort port, bool dualStack, NetworkConfiguration configuration)
        : base(server, address, port, dualStack, configuration)
    {

    }

    #endregion

    #region Get-/Setters

    public override bool Secure => false;

    #endregion

    #region Functionality

    protected override async ValueTask Accept(Socket client)
    {
        await using var socketReader = new SocketPipeReader(client);
        
        await Handle(client, new NetworkStream(client), socketReader.Reader);
    }

    #endregion

}
