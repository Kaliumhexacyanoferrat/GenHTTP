using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Core.Security;

using Infra = GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core
{
    
    public static class ServerBuilderExtensions
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
                host.SecureUpgrade(Infra.SecureUpgrade.Force);
            }

            if (strictTransport)
            {
                host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            return host;
        }

        public static IServerHost SecureUpgrade(this IServerHost host, SecureUpgrade mode)
        {
            if (mode != Infra.SecureUpgrade.None)
            {
                host.Add(new SecureUpgradeConcernBuilder().Mode(mode));
            }

            return host;
        }

        public static IServerHost StrictTransport(this IServerHost host, StrictTransportPolicy policy)
        {
            host.Add(new StrictTransportConcernBuilder().Policy(policy));
            return host;
        }

    }

}
