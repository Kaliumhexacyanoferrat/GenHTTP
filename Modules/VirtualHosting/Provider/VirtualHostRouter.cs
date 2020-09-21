using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.VirtualHosting.Provider
{

    public class VirtualHostRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, IHandler> Hosts { get; }

        private IHandler? DefaultRoute { get; }

        #endregion

        #region Initialization

        public VirtualHostRouter(IHandler parent,
                                 Dictionary<string, IHandlerBuilder> hosts,
                                 IHandlerBuilder? defaultRoute)
        {
            Parent = parent;

            Hosts = hosts.ToDictionary(kv => kv.Key, kv => kv.Value.Build(this));
            DefaultRoute = defaultRoute?.Build(this);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return GetRouter(request)?.Handle(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var router = GetRouter(request);

            return router?.GetContent(request) ?? new List<ContentElement>();
        }

        private IHandler? GetRouter(IRequest request)
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
