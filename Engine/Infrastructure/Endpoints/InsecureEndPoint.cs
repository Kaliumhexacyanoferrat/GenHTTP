using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Infrastructure.Configuration;

namespace GenHTTP.Engine.Infrastructure.Endpoints
{

    internal class InsecureEndPoint : EndPoint
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

        protected override Task Accept(Socket client) => Handle(client, new BufferedStream(new NetworkStream(client)));
        
        #endregion

    }

}
