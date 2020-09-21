using System;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Providers;

namespace GenHTTP.Modules.Practices
{

    public static class Extensions
    {

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
                host.SecureUpgrade(Api.Infrastructure.SecureUpgrade.Force);
            }

            if (strictTransport)
            {
                host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            return host;
        }

    }

}
