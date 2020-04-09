using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.Websites
{

    public class WebsiteRouter : IHandler, IErrorHandler, IPageRenderer
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IMenuProvider Menu { get; }

        private ITheme Theme { get; }

        private IHandler Handler { get; }

        private WebsiteRenderer Renderer { get; }

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

            var layout = Layout.Create()
                               .Section("scripts", scripts)
                               .Section("styles", styles)
                               .Section("sitemaps", Sitemap.Create())
                               .File("robots.txt", Robots.Default().Sitemap())
                               .Fallback(content);

            if (favicon != null)
            {
                layout.File("favicon.ico", Download.From(favicon).Type(ContentType.ImageIcon));
            }

            if (theme.Resources != null)
            {
                layout.Section("resources", theme.Resources);
            }

            Handler = layout.Build(this);

            Theme = theme;

            Menu = menu ?? Core.Menu.From(content.Build(this)).Build();

            // ToDO
            var scriptRouter = (ScriptRouter)scripts.Build(this);
            var styleRouter = (StyleRouter)styles.Build(this);

            Renderer = new WebsiteRenderer(Theme, Menu, scriptRouter, styleRouter);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            // ToDo: Error handling as a concern on the layout (can be reused everywhere ... corerouter etc.)

            try
            {
                var response = Handler.Handle(request);

                if (response == null)
                {
                    return this.NotFound(request)
                               .Build();
                }

                return response;
            }
            catch (Exception e)
            {
                var model = new ErrorModel(request, ResponseStatus.InternalServerError, 
                                           "Internal Server Error", "The server failed to handle this request.", e);

                return this.Error(model)
                           .Build();
            }
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

        public TemplateModel Render(ErrorModel error)
        {
            return new TemplateModel(error.Request, error.Title ?? "Error", Theme.ErrorHandler.Render(error));
        }

        public IResponseBuilder Render(TemplateModel model)
        {
            return model.Request.Respond()
                                .Content(Renderer.Render(model))
                                .Type(ContentType.TextHtml);
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
