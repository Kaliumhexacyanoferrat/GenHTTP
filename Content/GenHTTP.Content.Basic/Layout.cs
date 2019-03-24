using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Content.Basic
{

    public class Layout : IRouter
    {

        #region Get-/Setters

        public IRouter Parent { get; set; }

        private Dictionary<string, IRouter>? Routes { get; }

        private Dictionary<string, IContentProvider>? Content { get; }
        
        private string? Index { get; }

        private Func<bool, IContentPage>? Template { get; }

        private Func<ResponseType, IRoutingContext, IContentProvider>? ErrorHandler { get; }

        #endregion

        #region Initialization

        public Layout(Dictionary<string, IRouter>? routes = null,
                      Dictionary<string, IContentProvider>? content = null,
                      string? index = null,
                      Func<bool, IContentPage>? template = null,
                      Func<ResponseType, IRoutingContext, IContentProvider>? errorHandler = null)
        {
            Routes = routes;
            Content = content;

            Index = index;
            
            Template = template;
            ErrorHandler = errorHandler;

            if (routes != null)
            {
                foreach (var route in routes)
                {
                    route.Value.Parent = this;
                }
            }
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            // rewrite to index if requested
            if (current.IsIndex && Index != null)
            {
                current.Rewrite(Index);
            }

            // is there a matching content provider?
            if (Content != null)
            {
                var requested = current.ScopedPath;

                if (Content.ContainsKey(requested))
                {
                    current.RegisterContent(Content[requested]);
                    return;
                }
            }

            // are there any matching routes? 
            if (Routes != null)
            {
                var route = current.ScopeSegment(this);

                if (route != null)
                {
                    if (Routes.ContainsKey(route))
                    {
                        Routes[route].HandleContext(current);
                        return;
                    }
                }
            }
        }

        public IContentPage GetPage(bool error)
        {
            if (Template != null)
            {
                return Template(error);
            }

            return Parent.GetPage(error);
        }

        public IContentProvider GetProvider(ResponseType responseType, IRoutingContext context)
        {
            if (ErrorHandler != null)
            {
                return ErrorHandler(responseType, context);
            }

            return Parent.GetProvider(responseType, context);
        }

        #endregion

    }

}
