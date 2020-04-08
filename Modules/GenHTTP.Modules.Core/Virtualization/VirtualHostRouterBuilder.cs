using GenHTTP.Api.Content;
using System;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.Virtualization
{

    public class VirtualHostRouterBuilder : IHandlerBuilder
    {
        private readonly Dictionary<string, IHandlerBuilder> _Hosts = new Dictionary<string, IHandlerBuilder>();

        private IHandlerBuilder? _DefaultRoute;

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

        public VirtualHostRouterBuilder Default(IHandlerBuilder handler)
        {
            _DefaultRoute = handler;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return new VirtualHostRouter(parent, _Hosts, _DefaultRoute);
        }

        #endregion

    }

}
