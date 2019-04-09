using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Virtualization
{

    public static class VirtualizationExtensions
    {

        public static string? HostWithoutPort(this IRequest request)
        {
            var host = request.Host;

            if (host != null)
            {
                var pos = host.IndexOf(':');

                if (pos > 0)
                {
                    return host.Substring(0, pos);
                }
                else
                {
                    return host;
                }
            }

            return null;
        }

    }

}
