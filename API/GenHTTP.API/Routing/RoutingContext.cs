using System;
using System.Collections.Generic;
using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    public class RoutingContext : IEditableRoutingContext
    {

        #region Get-/Setters

        public IRouter Router { get; protected set; }

        public IContentProvider? ContentProvider { get; protected set; }

        public IHttpRequest Request { get; }

        public string ScopedPath { get; protected set; }

        public bool IsIndex => string.IsNullOrEmpty(ScopedPath) || ScopedPath == "/";

        #endregion

        #region Initialization

        public RoutingContext(IRouter router, IHttpRequest request)
        {
            Request = request;
            Router = router;

            ScopedPath = request.Path;
        }

        #endregion

        #region Functionality

        public void Scope(IRouter router)
        {
            Router = router;
        }

        public string? ScopeSegment(IRouter router)
        {
            var index = ScopedPath.Substring(1).IndexOf('/');

            if (index > -1)
            {
                var segment = ScopedPath.Substring(1, index);
                ScopedPath = ScopedPath.Substring(segment.Length + 1);

                return segment;
            }

            return null;
        }

        public void RegisterContent(IContentProvider contentProvider)
        {
            ContentProvider = contentProvider;
        }
        
        public void Rewrite(string relativeUrl)
        {
            ScopedPath = '/' + relativeUrl;
        }

        public string? Route(string route)
        {
            return "./" + route; // ToDo :)
        }

        #endregion

    }

}
