using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Websites
{

    public class WebsiteModel : IBaseModel
    {

        #region Get-/Setters

        public TemplateModel Content { get; }

        public List<ScriptReference> Scripts { get; }

        public List<StyleReference> Styles { get; }

        public IRequest Request { get; }

        public ITheme Theme { get; }

        public object? Model { get; }

        public List<ContentElement> Menu { get; }
        
        #endregion

        #region Initialization

        public WebsiteModel(IRequest request,
                            TemplateModel content,
                            ITheme theme,
                            object? themeModel,
                            List<ContentElement> menu,
                            List<ScriptReference> scripts,
                            List<StyleReference> styles)
        {
            Request = request;

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
