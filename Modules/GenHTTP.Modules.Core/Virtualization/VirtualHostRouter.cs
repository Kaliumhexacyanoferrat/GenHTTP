using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Virtualization
{

    public class VirtualHostRouter : RouterBase
    {

        #region Get-/Setters

        private Dictionary<string, IRouter> Hosts { get; }

        private IRouter? DefaultRoute { get; }

        #endregion

        #region Initialization

        public VirtualHostRouter(Dictionary<string, IRouter> hosts,
                                 IRouter? defaultRoute,
                                 IRenderer<TemplateModel>? template,
                                 IContentProvider? errorHandler) : base(template, errorHandler)
        {
            foreach (var router in hosts.Values)
            {
                router.Parent = this;
            }

            Hosts = hosts;

            if (defaultRoute != null)
            {
                defaultRoute.Parent = this;
            }

            DefaultRoute = defaultRoute;
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var host = current.Request.HostWithoutPort();

            // try to find a regular route
            if (host != null)
            {
                if (Hosts.ContainsKey(host))
                {
                    Hosts[host].HandleContext(current);
                    return;
                }
            }

            // route by default
            if (DefaultRoute != null)
            {
                DefaultRoute.HandleContext(current);
            }
        }

        public override string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }

        #endregion

    }

}
