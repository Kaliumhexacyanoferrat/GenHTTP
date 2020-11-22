using System.Collections.Generic;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    public sealed class WebsiteModel : IBaseModel
    {

        #region Get-/Setters

        public TemplateModel Content { get; }

        public List<ScriptReference> Scripts { get; }

        public List<StyleReference> Styles { get; }

        public IRequest Request { get; }

        public IHandler Handler { get; }

        public ITheme Theme { get; }

        public object? Model { get; }

        public List<ContentElement> Menu { get; }

        #endregion

        #region Initialization

        public WebsiteModel(IRequest request,
                            IHandler handler,
                            TemplateModel content,
                            ITheme theme,
                            object? themeModel,
                            List<ContentElement> menu,
                            List<ScriptReference> scripts,
                            List<StyleReference> styles)
        {
            Request = request;
            Handler = handler;

            Content = content;
            Scripts = scripts;
            Styles = styles;

            Menu = menu;

            Theme = theme;
            Model = themeModel;
        }

        #endregion

    }

}
