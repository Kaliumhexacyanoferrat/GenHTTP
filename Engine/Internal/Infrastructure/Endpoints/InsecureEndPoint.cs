using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Internal.Utilities;
using GenHTTP.Engine.Shared.Infrastructure;

using PooledAwait;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal sealed class InsecureEndPoint : EndPoint
{

    #region Initialization

    internal InsecureEndPoint(IServer server, IPAddress? address, ushort port, NetworkConfiguration configuration)
        : base(server, address, port, configuration)
    {

    }

    #endregion

    #region Get-/Setters

    public override bool Secure => false;

    #endregion

    #region Functionality

    protected override PooledValueTask Accept(Socket client) => Handle(client, new PoolBufferedStream(new NetworkStream(client), Configuration.TransferBufferSize));

    #endregion

}
