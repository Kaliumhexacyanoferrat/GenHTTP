using System.Collections.Generic;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    /// <summary>
    /// All information that is needed by a theme to render
    /// a web page for a website.
    /// </summary>
    public sealed class WebsiteModel : IBaseModel
    {

        #region Get-/Setters

        /// <summary>
        /// The content which needs to be embedded into the 
        /// generated website.
        /// </summary>
        public TemplateModel Content { get; }

        /// <summary>
        /// The JavaScript files which need to be provided
        /// to the browser.
        /// </summary>
        public List<ScriptReference> Scripts { get; }

        /// <summary>
        /// The stylesheets which need to be provided
        /// to the browser.
        /// </summary>
        public List<StyleReference> Styles { get; }

        /// <summary>
        /// The currently handled request.
        /// </summary>
        public IRequest Request { get; }

        /// <summary>
        /// The currently executed handler.
        /// </summary>
        public IHandler Handler { get; }

        /// <summary>
        /// The theme that is responsible to generate
        /// the page.
        /// </summary>
        public ITheme Theme { get; }

        /// <summary>
        /// The custom model provided by the theme, if any.
        /// </summary>
        public object? Model { get; }

        /// <summary>
        /// The menu to be rendered.
        /// </summary>
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
