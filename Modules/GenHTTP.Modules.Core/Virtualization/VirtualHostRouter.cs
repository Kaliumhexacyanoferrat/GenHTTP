using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
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
            
            GetRouter(current.Request)?.HandleContext(current);
        }

        public override string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            var router = GetRouter(request);

            return router?.GetContent(request, basePath) ?? new List<ContentElement>();
        }

        private IRouter? GetRouter(IRequest request)
        {
            var host = request.HostWithoutPort();

            // try to find a regular route
            if (host != null)
            {
                if (Hosts.ContainsKey(host))
                {
                    return Hosts[host];
                }
            }

            // route by default
            return DefaultRoute;
        }

        #endregion

    }

}
