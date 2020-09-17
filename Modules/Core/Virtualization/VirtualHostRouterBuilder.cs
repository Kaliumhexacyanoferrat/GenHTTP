using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Core.Virtualization
{

    public class VirtualHostRouterBuilder : IHandlerBuilder<VirtualHostRouterBuilder>
    {
        private readonly Dictionary<string, IHandlerBuilder> _Hosts = new Dictionary<string, IHandlerBuilder>();

        private IHandlerBuilder? _DefaultRoute;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public VirtualHostRouterBuilder Add(string host, IHandlerBuilder handler)
        {
            if (_Hosts.ContainsKey(host))
            {
                throw new InvalidOperationException("A host with this name has already been added");
            }

            _Hosts.Add(host, handler);
            return this;
        }

        public VirtualHostRouterBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public VirtualHostRouterBuilder Default(IHandlerBuilder handler)
        {
            _DefaultRoute = handler;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new VirtualHostRouter(p, _Hosts, _DefaultRoute));
        }

        #endregion

    }

}
