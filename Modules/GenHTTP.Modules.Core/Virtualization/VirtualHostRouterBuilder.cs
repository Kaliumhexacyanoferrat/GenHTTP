using System;
using System.Collections.Generic;

using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Virtualization
{

    public class VirtualHostRouterBuilder : RouterBuilderBase<VirtualHostRouterBuilder>
    {
        private Dictionary<string, IRouter> _Hosts = new Dictionary<string, IRouter>();
        private IRouter? _DefaultRoute;

        #region Functionality

        public VirtualHostRouterBuilder Add(string host, IRouterBuilder router)
        {
            return Add(host, router.Build());
        }

        public VirtualHostRouterBuilder Add(string host, IRouter router)
        {
            if (_Hosts.ContainsKey(host))
            {
                throw new InvalidOperationException("A host with this name has already been added");
            }

            _Hosts.Add(host, router);
            return this;
        }

        public VirtualHostRouterBuilder Default(IRouterBuilder router)
        {
            return Default(router.Build());
        }

        public VirtualHostRouterBuilder Default(IRouter router)
        {
            _DefaultRoute = router;
            return this;
        }

        public override IRouter Build()
        {
            return new VirtualHostRouter(_Hosts, _DefaultRoute, _Template, _ErrorHandler);
        }

        #endregion

    }

}
