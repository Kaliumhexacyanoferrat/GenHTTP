using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    public class RoutingContext : IEditableRoutingContext
    {
        private static readonly Regex URI_START = new Regex("^[a-z]+:");

        #region Get-/Setters

        public IRouter Router { get; protected set; }

        public IContentProvider? ContentProvider { get; protected set; }

        public IRequest Request { get; }

        public string ScopedPath { get; protected set; }
        
        #endregion

        #region Initialization

        public RoutingContext(IRouter router, IRequest request)
        {
            Request = request;
            Router = router;

            ScopedPath = request.Path;
        }

        #endregion

        #region Functionality

        public void Scope(IRouter router, string? segment = null)
        {
            Router = router;

            if (segment != null)
            {
                ScopedPath = (ScopedPath.Length > 1) ? ScopedPath.Substring(segment.Length + 1) : "/";
            }
        }
                        
        public void RegisterContent(IContentProvider contentProvider)
        {
            ContentProvider = contentProvider;
        }
        
        public string? Route(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                return null;
            }

            if (route.StartsWith(".") || route.StartsWith("/"))
            {
                return route;
            }

            if (URI_START.IsMatch(route))
            {
                return route;
            }

            var depth = ScopedPath.Count(f => f == '/') - 1;

            return Router.Route(route, Math.Max(0, depth));
        }
        
        #endregion

    }

}
