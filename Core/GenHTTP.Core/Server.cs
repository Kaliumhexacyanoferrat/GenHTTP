using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Infrastructure;

namespace GenHTTP.Core
{

    /// <summary>
    /// Allows to create server instances.
    /// </summary>
    public static class Server
    {

        /// <summary>
        /// Create a new, configurable server instance with
        /// default values.
        /// </summary>
        /// <returns>The builder to create the instance</returns>
        public static IServerBuilder Create()
        {
            return new ThreadedServerBuilder();
        }

    }

}
