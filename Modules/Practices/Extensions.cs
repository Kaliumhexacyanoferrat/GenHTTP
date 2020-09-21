using System;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Providers;

namespace GenHTTP.Modules.Practices
{

    public static class Extensions
    {

        /// <summary>
        /// Configures the server host with default policies for compression and security.
        /// </summary>
        /// <param name="host">The host to be configured</param>
        /// <param name="compression">Whether responses sent by the server should automatically be compressed</param>
        /// <param name="secureUpgrade">Whether the server should automatically upgrade insecure requests</param>
        /// <param name="strictTransport">Whether the server should send a strict transport policy</param>
        public static IServerHost Defaults(this IServerHost host,
                                           bool compression = true,
                                           bool secureUpgrade = true,
                                           bool strictTransport = true)
        {
            if (compression)
            {
                host.Compression(CompressedContent.Default());
            }

            if (secureUpgrade)
            {
                host.SecureUpgrade(SecureUpgrade.Force);
            }

            if (strictTransport)
            {
                host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            return host;
        }

    }

}
