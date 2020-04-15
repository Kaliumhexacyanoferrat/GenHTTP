using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

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
                             IEnumerable<IConcernBuilder> concerns,
                             IHandlerBuilder scripts,
                             IHandlerBuilder styles,
                             IResourceProvider? favicon,
                             IMenuProvider? menu,
                             ITheme theme)
        {
            Parent = parent;

            var layout = Layout.Create()
                               .Add("scripts", scripts)
                               .Add("styles", styles)
                               .Add("sitemap.xml", Sitemap.Create())
                               .Add("robots.txt", Robots.Default().Sitemap())
                               .Fallback(content);

            foreach (var concern in concerns)
            {
                layout.Add(concern);
            }

            if (favicon != null)
            {
                layout.Add("favicon.ico", Download.From(favicon).Type(ContentType.ImageIcon));
            }

            if (theme.Resources != null)
            {
                layout.Add("resources", theme.Resources);
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

        public IResponse? Handle(IRequest request) => Handler.Handle(request);

        public IEnumerable<ContentElement> GetContent(IRequest request) => Handler.GetContent(request);

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
