using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Core;

using GenHTTP.Hosting.Embedded.Routing;

namespace GenHTTP.Hosting.Embedded
{

    public class EmbeddedServer : Server
    {

        #region Initialization

        internal EmbeddedServer(IRouter router, IServerCompanion? companion, int port = 80, int backlog = 20) 
            : base(router, companion, port, backlog)
        {

        } 

        public static EmbeddedServer Run(IRouter router, IServerCompanion? companion, int port = 80, int backlog = 20)
        {
            return new EmbeddedServer(new EmbeddedRouter(router), companion, port, backlog);
        }

        #endregion

    }

}
