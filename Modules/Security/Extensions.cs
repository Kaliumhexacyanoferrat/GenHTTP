using System;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Security.Providers;

namespace GenHTTP.Modules.Security
{

    public static class Extensions
    {


        public static IServerHost Harden(this IServerHost host,
                                         bool secureUpgrade = true,
                                         bool strictTransport = true,
                                         bool preventSniffing = true)
        {
            if (secureUpgrade)
            {
                host.SecureUpgrade(Api.Infrastructure.SecureUpgrade.Force);
            }

            if (strictTransport)
            {
                host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            if (preventSniffing)
            {
                host.PreventSniffing();
            }

            return host;
        }

        public static IServerHost SecureUpgrade(this IServerHost host, SecureUpgrade mode)
        {
            if (mode != Api.Infrastructure.SecureUpgrade.None)
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

        public static IServerHost PreventSniffing(this IServerHost host)
        {
            host.Add(new SnifferPreventionConcernBuilder());
            return host;
        }

    }

}
