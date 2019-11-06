using System.Collections.Generic;

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

        private IRouter? DefaultRouter { get; }

        private IContentProvider? DefaultContent { get; }

        #endregion

        #region Initialization

        public LayoutRouter(Dictionary<string, IRouter> routes,
                            Dictionary<string, IContentProvider> content,
                            IRouter? defaultRouter,
                            IContentProvider? defaultContent,
                            IRenderer<TemplateModel>? template,
                            IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Routes = routes;
            DefaultRouter = defaultRouter;

            Content = content;
            DefaultContent = defaultContent;

            foreach (var route in routes)
            {
                route.Value.Parent = this;
            }

            if (defaultRouter != null)
            {
                defaultRouter.Parent = this;
            }
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            var segment = Api.Routing.Route.GetSegment(current.ScopedPath);

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

            // route by default
            if (DefaultRouter != null)
            {
                current.Scope(this);
                DefaultRouter.HandleContext(current);
                return;
            }

            // default content
            if (DefaultContent != null)
            {
                current.Scope(this);
                current.RegisterContent(DefaultContent);
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
                return Api.Routing.Route.GetRelation(currentDepth) + path;
            }

            return Parent.Route(path, currentDepth + 1);
        }

        #endregion

    }

}
