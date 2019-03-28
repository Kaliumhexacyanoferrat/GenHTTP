using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Infrastructure;

namespace GenHTTP.Core
{

    public static class Server
    {

        public static IServerBuilder Create()
        {
            return new ThreadedServerBuilder();
        }

    }

}
