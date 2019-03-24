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

        private ITemplateProvider? Template { get; }

        private IErrorHandler? ErrorHandler { get; }

        #endregion

        #region Initialization

        public Layout(Dictionary<string, IRouter>? routes = null,
                      Dictionary<string, IContentProvider>? content = null,
                      string? index = null,
                      ITemplateProvider? template = null,
                      IErrorHandler? errorHandler = null)
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
                var requested = current.ScopedPath.Substring(1);

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

        public IContentPage GetPage(IHttpRequest request, IHttpResponse response)
        {
            if (Template != null)
            {
                return Template.GetPage(request, response);
            }

            return Parent.GetPage(request, response);
        }

        public IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response)
        {
            if (ErrorHandler != null)
            {
                return ErrorHandler.GetContent(request, response);
            }

            return Parent.GetErrorHandler(request, response);
        }
        
        #endregion

    }

}
