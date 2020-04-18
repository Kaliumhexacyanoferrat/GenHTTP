using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;

namespace GenHTTP.Modules.Core.Websites
{

    public class WebsiteRenderer : IRenderer<TemplateModel>
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

        public string Render(TemplateModel model)
        {
            var menu = Menu.GetMenu(model.Request);

            var themeModel = Theme.GetModel(model.Request);

            var bundle = !model.Request.Server.Development;

            var websiteModel = new WebsiteModel(model.Request, model.Handler, model, Theme, themeModel, menu, Scripts.GetReferences(bundle), Styles.GetReferences(bundle));

            return Theme.Renderer.Render(websiteModel);
        }

        #endregion

    }

}
