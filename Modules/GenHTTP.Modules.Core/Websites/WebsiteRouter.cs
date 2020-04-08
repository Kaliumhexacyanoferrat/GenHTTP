using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class WebsiteRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private IHandler Content { get; }

        private IHandler Scripts { get; }

        private IHandler Styles { get; }

        private IHandler? Favicon { get; }

        private IHandler? Robots { get; }

        private IHandler? Sitemaps { get; }

        public IMenuProvider Menu { get; }

        private IHandler? Resources { get; }

        private ITheme Theme { get; }

        #endregion

        #region Initialization

        public WebsiteRouter(IHandler parent,
                             IHandlerBuilder content,
                             IHandlerBuilder scripts,
                             IHandlerBuilder styles,
                             IResourceProvider? favicon,
                             IMenuProvider? menu,
                             ITheme theme)
        {
            Parent = parent;

            Content = content.Build(this);

            Scripts = scripts.Build(this);
            Styles = styles.Build(this);

            Sitemaps = Sitemap.Create()
                              .Build(this);

            Robots = Core.Robots.Default()
                                .Sitemap()
                                .Build(this);

            if (favicon != null)
            {
                Favicon = Download.From(favicon)
                                  .Type(ContentType.ImageIcon)
                                  .Build(this);
            }

            Resources = theme.Resources?.Build(this);

            Theme = theme;

            Menu = menu ?? Core.Menu.From(Content).Build();
        }

        #endregion

        #region Functionality

        // ToDo: Proper error handling via interfaces

        /*public IRenderer<TemplateModel> GetRenderer()
        {
            return new WebsiteRenderer(Theme, Menu, Scripts, Styles);
        }

        public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            return ErrorHandler ?? Theme.GetErrorHandler(request, responseType, cause) ?? Parent.GetErrorHandler(request, responseType, cause);
        }*/

        public IResponse? Handle(IRequest request)
        {
            /*current.Scope(this);

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
            else if (segment == "favicon.ico" && Favicon != null)
            {
                current.RegisterContent(Favicon);
            }
            else if (segment == "robots.txt" && Robots != null)
            {
                current.RegisterContent(Robots);
            }
            else if (segment == "sitemaps" && Sitemaps != null)
            {
                current.Scope(Sitemaps, segment);
                Sitemaps.HandleContext(current);
            }
            else
            {
                Content.HandleContext(current);
            }*/

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            /*foreach (var script in Scripts.GetContent(request, $"{basePath}scripts/"))
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

            if (Favicon != null)
            {
                yield return new ContentElement($"{basePath}favicon.ico", "Favicon", ContentType.ImageIcon, null);
            }

            if (Robots != null)
            {
                yield return new ContentElement($"{basePath}robots.txt", "Robots Instruction File", ContentType.TextPlain, null);
            }

            if (Sitemaps != null)
            {
                foreach (var sitemap in Sitemaps.GetContent(request, $"{basePath}sitemaps/"))
                {
                    yield return sitemap;
                }
            }

            foreach (var content in Content.GetContent(request, basePath))
            {
                yield return content;
            }*/

            return new List<ContentElement>();
        }

        /*public string? Route(string path, int currentDepth)
        {
            var segment = Api.Routing.Route.GetSegment(path);

            if (segment == "scripts" || segment == "styles" || segment == "resources" || segment == "sitemaps" || segment == "favicon.ico" || segment == "robots.txt")
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
        }*/

        #endregion

    }

}
