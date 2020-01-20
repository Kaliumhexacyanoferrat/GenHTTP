using System;
using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Proxy
{

    public class ReverseProxyRouter : RouterBase, IRouter
    {

        #region Get-/Setters

        private ReverseProxyProvider Provider { get; }

        #endregion

        #region Initialization

        public ReverseProxyRouter(string upstream, TimeSpan connectTimeout, TimeSpan readTimeout, IContentProvider? errorHandler) : base(null, errorHandler)
        {
            Provider = new ReverseProxyProvider(upstream, connectTimeout, readTimeout, null);
        }

        #endregion

        #region Functionality

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath) => new List<ContentElement>();

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.RegisterContent(Provider);
            current.Scope(this);
        }

        public override string? Route(string path, int currentDepth) => Parent.Route(path, currentDepth);

        #endregion

    }

}
