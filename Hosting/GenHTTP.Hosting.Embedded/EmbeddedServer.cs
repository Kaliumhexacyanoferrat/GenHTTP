using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;
using GenHTTP.Core;
using GenHTTP.Hosting.Embedded.Routing;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Hosting.Embedded
{

    public class EmbeddedServer : Server, IEmbeddedServer
    {

        #region Initialization

        internal EmbeddedServer(IRouter router, ILoggerFactory loggerFactory, int port = 80, int backlog = 20) 
            : base(router, loggerFactory, port, backlog)
        {

        } 

        public static EmbeddedServer Run(IRouter router, ILoggerFactory loggerFactory, int port = 80, int backlog = 20)
        {
            return new EmbeddedServer(new EmbeddedRouter(router), loggerFactory, port, backlog);
        }

        #endregion

    }

}
