using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal class InsecureEndPoint : EndPoint
    {

        #region Get-/Setters

        public override bool Secure => false;

        #endregion

        #region 

        internal InsecureEndPoint(IServer server, IPEndPoint endPoint, NetworkConfiguration configuration) 
            : base(server, endPoint, configuration)
        {

        }

        #endregion

        #region Functionality

        protected override async Task Accept(Socket client)
        {
            await Handle(client, new NetworkStream(client));
        }

        #endregion

    }

}
