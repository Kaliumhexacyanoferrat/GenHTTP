using System;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.IO;
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
        /// <param name="clientCaching">Validates the cached entries of the client by sending an ETag header and evaluating it when a request is processed (returning HTTP 304 if the content did not change)</param>
        /// <param name="rangeSupport">Enables partial responses if requested by the client</param>
        /// <param name="preventSniffing">Instructs clients not to guess the MIME type of the served content</param>
        public static IServerHost Defaults(this IServerHost host,
                                           bool compression = true,
                                           bool secureUpgrade = true,
                                           bool strictTransport = true,
                                           bool clientCaching = true,
                                           bool rangeSupport = false,
                                           bool preventSniffing = false)
        {
            if (strictTransport)
            {
                host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            if (compression)
            {
                host.Compression(CompressedContent.Default());
            }

            if (rangeSupport)
            {
                host.RangeSupport();
            }

            if (clientCaching)
            {
                host.ClientCaching();
            }

            if (secureUpgrade)
            {
                host.SecureUpgrade(SecureUpgrade.Force);
            }

            if (preventSniffing)
            {
                host.PreventSniffing();
            }

            return host;
        }

    }

}
