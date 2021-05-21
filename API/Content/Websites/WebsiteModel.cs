using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    /// <summary>
    /// All information that is needed by a theme to render
    /// a web page for a website.
    /// </summary>
    public sealed class WebsiteModel : AbstractModel
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
                            List<StyleReference> styles) : base(request, handler)
        {
            Content = content;
            Scripts = scripts;
            Styles = styles;

            Menu = menu;

            Theme = theme;
            Model = themeModel;
        }

        #endregion

        #region Functionality

        public override ValueTask<ulong> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + (uint)Content.Content.GetHashCode();
                hash = hash * 23 + (uint)Content.Meta.GetHashCode();

                foreach (var script in Scripts)
                {
                    hash = hash * 23 + (uint)script.GetHashCode();
                }

                foreach (var style in Styles)
                {
                    hash = hash * 23 + (uint)style.GetHashCode();
                }

                foreach (var item in Menu)
                {
                    hash = hash * 23 + item.CalculateChecksum();
                }

                hash = hash * 23 + (uint)(Model?.GetHashCode() ?? 0);

                return new(hash);
            }
        }

        #endregion

    }

}
