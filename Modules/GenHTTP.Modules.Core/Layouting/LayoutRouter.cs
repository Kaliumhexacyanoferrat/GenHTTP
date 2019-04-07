using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Layouting
{

    public class LayoutRouter : RouterBase
    {

        #region Get-/Setters
        
        private Dictionary<string, IRouter> Routes { get; }

        private Dictionary<string, IContentProvider> Content { get; }

        private string? Index { get; }
        
        #endregion

        #region Initialization

        public LayoutRouter(Dictionary<string, IRouter> routes,
                            Dictionary<string, IContentProvider> content,
                            string? index,
                            IRenderer<TemplateModel>? template,
                            IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Routes = routes;
            Content = content;

            Index = index;
            
            foreach (var route in routes)
            {
                route.Value.Parent = this;
            }
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
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
        
        public override string? Route(string path, int currentDepth)
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
