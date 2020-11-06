using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.Websites.Resources;

namespace GenHTTP.Modules.Websites.Sites
{

    public class WebsiteRouter : IHandler, IErrorHandler, IPageRenderer, IHandlerResolver
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
                             IResource? favicon,
                             IMenuProvider? menu,
                             ITheme theme)
        {
            Parent = parent;

            var layout = Layout.Create()
                               .Add("scripts", scripts)
                               .Add("styles", styles)
                               .Add("sitemap.xml", Sitemap.Create())
                               .Add("robots.txt", BotInstructions.Default().Sitemap())
                               .Fallback(content);

            foreach (var concern in concerns)
            {
                layout.Add(concern);
            }

            if (favicon != null)
            {
                layout.Add("favicon.ico", Content.From(favicon));
            }

            if (theme.Resources != null)
            {
                layout.Add("resources", theme.Resources);
            }

            Handler = layout.Build(this);

            Theme = theme;

            Menu = menu ?? Websites.Menu.Create((r, _) => GetContent(r)).Build();

            var scriptRouter = (ScriptRouter)scripts.Build(this);
            var styleRouter = (StyleRouter)styles.Build(this);

            Renderer = new WebsiteRenderer(Theme, Menu, scriptRouter, styleRouter);
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Handler.HandleAsync(request);

        public IEnumerable<ContentElement> GetContent(IRequest request) => Handler.GetContent(request);

        public async ValueTask<TemplateModel> RenderAsync(ErrorModel error, ContentInfo details)
        {
            return new TemplateModel(error.Request, error.Handler, details, await Theme.ErrorHandler.RenderAsync(error).ConfigureAwait(false));
        }

        public async ValueTask<IResponseBuilder> RenderAsync(TemplateModel model)
        {
            var response = await model.Request.Respond()
                                              .SetContentAsync(await Renderer.RenderAsync(model).ConfigureAwait(false))
                                              .ConfigureAwait(false);

            return response.Type(ContentType.TextHtml);
        }

        public IHandler? Find(string segment)
        {
            if (segment == "{website}")
            {
                return this;
            }

            return null;
        }

        #endregion

    }

}
