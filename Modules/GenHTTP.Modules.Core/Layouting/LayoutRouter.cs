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
        private IRouter? _Parent;

        #region Get-/Setters

        public IRouter Parent
        {
            get { return _Parent ?? throw new InvalidOperationException("Parent has not been set"); }
            set { _Parent = value; }
        }

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
            var segment = Api.Routing.Route.GetSegment(current.ScopedPath);

            // rewrite to index if requested
            if (string.IsNullOrEmpty(segment) && Index != null)
            {
                segment = Index;
            }

            // is there a matching content provider?
            if (Content.ContainsKey(segment))
            {
                current.Scope(this, segment);
                current.RegisterContent(Content[segment]);
                return;
            }

            // are there any matching routes? 
            if (Routes.ContainsKey(segment))
            {
                current.Scope(this, segment);
                Routes[segment].HandleContext(current);
                return;
            }

            // no route found
            current.Scope(this);
        }

        public IRenderer<TemplateModel> GetRenderer()
        {
            return Template ?? Parent.GetRenderer();
        }

        public IContentProvider GetErrorHandler(IRequest request, ResponseType responseType)
        {
            return ErrorHandler ?? Parent.GetErrorHandler(request, responseType);
        }

        public string? Route(string path, int currentDepth)
        {
            var segment = Api.Routing.Route.GetSegment(path);

            if (Content.ContainsKey(segment) || Routes.ContainsKey(segment))
            {
                if (segment == Index)
                {
                    return Api.Routing.Route.GetRelation(currentDepth);
                }
                else
                {
                    return Api.Routing.Route.GetRelation(currentDepth) + path;
                }
            }

            return Parent.Route(path, currentDepth + 1);
        }

        #endregion

    }

}
