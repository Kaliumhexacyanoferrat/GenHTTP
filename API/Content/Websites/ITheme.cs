using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    /// <summary>
    /// Provides a theme that can be used to style a website.
    /// </summary>
    public interface ITheme
    {

        /// <summary>
        /// The JavaScript files which are required by this theme.
        /// </summary>
        /// <remarks>
        /// Script files can be accessed via "scripts/".
        /// </remarks>
        List<Script> Scripts { get; }

        /// <summary>
        /// The CSS stylesheets which are required by this theme.
        /// </summary>
        /// <remarks>
        /// Stylesheets can be accessed via "styles/".
        /// </remarks>
        List<Style> Styles { get; }

        /// <summary>
        /// Additional resources required by this theme.
        /// </summary>
        /// <remarks>
        /// Resources can be accessed via "resources/".
        /// </remarks>
        IHandlerBuilder? Resources { get; }

        /// <summary>
        /// The renderer used to provide theme-specific error pages.
        /// </summary>
        IRenderer<ErrorModel> ErrorHandler { get; }

        /// <summary>
        /// The renderer used to generate pages served by the website. 
        /// </summary>
        /// <remarks>
        /// The renderer should provide a complete HTML page which embedds
        /// the content stored in the given model.
        /// </remarks>
        IRenderer<WebsiteModel> Renderer { get; }

        /// <summary>
        /// Invoked to obtain a custom model that is required by the
        /// theme renderer to render the page.
        /// </summary>
        /// <param name="request">The request which is to be handled</param>
        /// <param name="handler">The handler which is currently executed</param>
        /// <returns>The model to be passed to the website renderer</returns>
        /// <remarks>
        /// The value returned by this method is stored in <see cref="WebsiteModel.Model"/>.
        /// </remarks>
        ValueTask<object?> GetModelAsync(IRequest request, IHandler handler);

    }

}
