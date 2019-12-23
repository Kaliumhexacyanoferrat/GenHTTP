using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Hosting;

namespace GenHTTP.Core
{

    public static class Host
    {

        /// <summary>
        /// Provides a new server host that can be used to run a
        /// server instance of the GenHTTP webserver.
        /// </summary>
        /// <returns>The host which can be used to run a server instance</returns>
        public static IServerHost Create() => new ServerHost();

    }

}
