using System;
using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.Websites
{

    public class WebsiteRouter : IRouter
    {
        private IRouter? _Parent;

        #region Get-/Setters

        private IRouter Content { get; }

        private ScriptRouter Scripts { get; }

        private StyleRouter Styles { get; }

        public IMenuProvider Menu { get; }

        private IRouter? Resources { get; }

        private ITheme Theme { get; }

        private IContentProvider? ErrorHandler { get; }

        public IRouter Parent
        {
            get { return _Parent ?? throw new InvalidOperationException("Parent has not been set"); }
            set { _Parent = value; }
        }

        #endregion

        #region Initialization

        public WebsiteRouter(IRouter content,
                             ScriptRouter scripts,
                             StyleRouter styles,
                             IMenuProvider menu,
                             ITheme theme,
                             IContentProvider? errorHandler)
        {
            Content = content;
            Content.Parent = this;

            Scripts = scripts;
            Scripts.Parent = this;

            Styles = styles;
            Styles.Parent = this;

            Resources = theme.Resources;

            if (Resources != null)
            {
                Resources.Parent = this;
            }

            ErrorHandler = errorHandler;
            Theme = theme;

            Menu = menu;
        }

        #endregion

        #region Functionality

        public IRenderer<TemplateModel> GetRenderer()
        {
            return new WebsiteRenderer(Theme, Menu, Scripts, Styles);
        }

        public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            return ErrorHandler ?? Theme.GetErrorHandler(request, responseType, cause) ?? Parent.GetErrorHandler(request, responseType, cause);
        }

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var segment = Api.Routing.Route.GetSegment(current.ScopedPath);

            if (segment == "scripts" && !Scripts.Empty)
            {
                current.Scope(Scripts, segment);
                Scripts.HandleContext(current);
            }
            else if (segment == "styles" && !Styles.Empty)
            {
                current.Scope(Styles, segment);
                Styles.HandleContext(current);
            }
            else if (segment == "resources" && Resources != null)
            {
                current.Scope(Resources, segment);
                Resources.HandleContext(current);
            }
            else
            {
                Content.HandleContext(current);
            }
        }

        public IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            foreach (var script in Scripts.GetContent(request, $"{basePath}scripts/"))
            {
                yield return script;
            }

            foreach (var style in Styles.GetContent(request, $"{basePath}styles/"))
            {
                yield return style;
            }

            foreach (var resource in Styles.GetContent(request, $"{basePath}resources/"))
            {
                yield return resource;
            }

            foreach (var content in Content.GetContent(request, basePath))
            {
                yield return content;
            }
        }

        public string? Route(string path, int currentDepth)
        {
            var segment = Api.Routing.Route.GetSegment(path);

            if (segment == "scripts" || segment == "styles" || segment == "resources")
            {
                return Api.Routing.Route.GetRelation(currentDepth - 1) + path;
            }

            if (segment == "{root}")
            {
                if (path != segment)
                {
                    return Api.Routing.Route.GetRelation(currentDepth - 1) + path.Substring(segment.Length + 1);
                }

                return Api.Routing.Route.GetRelation(currentDepth - 1);
            }

            return Parent.Route(path, currentDepth);
        }

        #endregion

    }

}
