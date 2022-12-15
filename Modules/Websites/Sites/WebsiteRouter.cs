using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Sitemaps;
using GenHTTP.Modules.Websites.Resources;

namespace GenHTTP.Modules.Websites.Sites
{

    public sealed class WebsiteRouter : IHandler, IErrorRenderer, IPageRenderer, IHandlerResolver
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
                               .Add(BotInstructions.FILE_NAME, BotInstructions.Default().Sitemap())
                               .Add(Sitemap.FILE_NAME, Sitemap.Create())
                               .Add(content);

            foreach (var concern in concerns)
            {
                layout.Add(concern);
            }

            // install a HTML error handler that will catch all exceptions
            // and render them using the IErrorRenderer implemented by this
            // handler
            layout.Add(ErrorHandler.Html());

            if (favicon is not null)
            {
                layout.Add("favicon.ico", Content.From(favicon));
            }

            if (theme.Resources is not null)
            {
                layout.Add("resources", theme.Resources);
            }

            Handler = layout.Build(this);

            Theme = theme;

            Menu = menu ?? Websites.Menu.Create((r, _) => GetContentAsync(r)).Build();

            var scriptRouter = (ScriptRouter)scripts.Build(this);
            var styleRouter = (StyleRouter)styles.Build(this);

            Renderer = new WebsiteRenderer(Theme, Menu, scriptRouter, styleRouter);
        }

        #endregion

        #region Functionality

        public async ValueTask PrepareAsync()
        {
            await Handler.PrepareAsync().ConfigureAwait(false);

            await Renderer.PrepareAsync();

            await Theme.ErrorHandler.PrepareAsync();

            await Theme.Renderer.PrepareAsync();
        }


        ValueTask<ulong> IRenderer<ErrorModel>.CalculateChecksumAsync() => Theme.ErrorHandler.CalculateChecksumAsync();

        ValueTask<ulong> IRenderer<TemplateModel>.CalculateChecksumAsync() => Renderer.CalculateChecksumAsync();

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Handler.HandleAsync(request);

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Handler.GetContentAsync(request);

        public async ValueTask<string> RenderAsync(ErrorModel model)
        {
            return await Theme.ErrorHandler.RenderAsync(model).ConfigureAwait(false);
        }

        public async ValueTask RenderAsync(ErrorModel model, Stream target)
        {
            await Theme.ErrorHandler.RenderAsync(model, target).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderAsync(TemplateModel model)
        {
            return await Renderer.RenderAsync(model).ConfigureAwait(false);
        }

        public async ValueTask RenderAsync(TemplateModel model, Stream target)
        {
            await Renderer.RenderAsync(model, target).ConfigureAwait(false);
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
