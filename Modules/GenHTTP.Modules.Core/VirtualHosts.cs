using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Modules.Core.Virtualization;

namespace GenHTTP.Modules.Core
{

    public static class VirtualHosts
    {

        public static VirtualHostRouterBuilder Create()
        {
            return new VirtualHostRouterBuilder();
        }

    }

}
