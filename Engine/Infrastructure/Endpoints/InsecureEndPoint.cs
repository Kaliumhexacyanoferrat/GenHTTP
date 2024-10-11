using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Utilities;

using PooledAwait;

namespace GenHTTP.Engine.Infrastructure.Endpoints
{

    internal sealed class InsecureEndPoint : EndPoint
    {

        #region Get-/Setters

        public override bool Secure => false;

        #endregion

        #region Initialization 

        internal InsecureEndPoint(IServer server, IPEndPoint endPoint, NetworkConfiguration configuration)
            : base(server, endPoint, configuration)
        {

        }

        #endregion

        #region Functionality

        protected override PooledValueTask Accept(Socket client) => Handle(client, new PoolBufferedStream(new NetworkStream(client), Configuration.TransferBufferSize));
        
        #endregion

    }

}
