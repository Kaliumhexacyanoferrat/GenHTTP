using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Infrastructure.Configuration;
using GenHTTP.Engine.Infrastructure.Transport;

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

        protected override PooledValueTask Accept(SocketConnection connection) => Handle(connection);
        
        #endregion

    }

}
