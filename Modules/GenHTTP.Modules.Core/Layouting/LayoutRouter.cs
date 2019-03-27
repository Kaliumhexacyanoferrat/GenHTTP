using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Layouting
{

    public class LayoutRouter : IRouter
    {
        
        #region Get-/Setters

        public IRouter Parent { get; set; }

        private Dictionary<string, IRouter> Routes { get; }

        private Dictionary<string, IContentProvider> Content { get; }

        private string? Index { get; }

        private IRenderer<TemplateModel>? Template { get; }

        private IContentProvider? ErrorHandler { get; }

        #endregion

        #region Initialization

        public LayoutRouter(Dictionary<string, IRouter> routes,
                            Dictionary<string, IContentProvider> content,
                            string? index,
                            IRenderer<TemplateModel>? template,
                            IContentProvider? errorHandler)
        {
            Routes = routes;
            Content = content;

            Index = index;

            Template = template;
            ErrorHandler = errorHandler;

            foreach (var route in routes)
            {
                route.Value.Parent = this;
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
            var requested = current.ScopedPath.Substring(1);

            if (Content.ContainsKey(requested))
            {
                current.RegisterContent(Content[requested]);
                return;
            }

            // are there any matching routes? 
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

        public IRenderer<TemplateModel> GetRenderer()
        {
            return Template ?? Parent.GetRenderer();
        }
        
        public IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response)
        {
            return ErrorHandler ?? Parent.GetErrorHandler(request, response);
        }

        #endregion

    }

}
