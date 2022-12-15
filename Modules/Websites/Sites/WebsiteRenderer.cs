using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;

using GenHTTP.Modules.Websites.Resources;

namespace GenHTTP.Modules.Websites.Sites
{

    public sealed class WebsiteRenderer : IRenderer<TemplateModel>
    {

        #region Get-/Setters

        private ITheme Theme { get; }

        private ScriptRouter Scripts { get; }

        private StyleRouter Styles { get; }

        public IMenuProvider Menu { get; }

        #endregion

        #region Initialization

        public WebsiteRenderer(ITheme theme,
                               IMenuProvider menu,
                               ScriptRouter scripts,
                               StyleRouter styles)
        {
            Theme = theme;

            Scripts = scripts;
            Styles = styles;

            Menu = menu;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong> CalculateChecksumAsync() => Theme.Renderer.CalculateChecksumAsync();

        public async ValueTask<string> RenderAsync(TemplateModel model)
        {
            return await Theme.Renderer.RenderAsync(await GetWebsiteModel(model).ConfigureAwait(false));
        }

        public async ValueTask RenderAsync(TemplateModel model, Stream target)
        {
            await Theme.Renderer.RenderAsync(await GetWebsiteModel(model).ConfigureAwait(false), target);
        }

        public ValueTask PrepareAsync() => Theme.Renderer.PrepareAsync();

        private async ValueTask<WebsiteModel> GetWebsiteModel(TemplateModel model)
        {
            var menu = await Menu.GetMenuAsync(model.Request, model.Handler).ConfigureAwait(false);

            var themeModel = await Theme.GetModelAsync(model.Request, model.Handler);

            var bundle = !model.Request.Server.Development;

            return new WebsiteModel(model.Request, model.Handler, model, Theme, themeModel, menu, Scripts.GetReferences(bundle), Styles.GetReferences(bundle));
        }

        #endregion

    }

}
