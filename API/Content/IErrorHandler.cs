using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Handlers implementing this interface will be queried to render
    /// errors that occur when handling requests.
    /// </summary>
    public interface IErrorHandler
    {

        /// <summary>
        /// Renders the given error into a web page that can be rendered
        /// using a template.
        /// </summary>
        /// <param name="error">The error to be handled</param>
        /// <param name="details">Additional details about the error</param>
        /// <returns>The page to be rendered into the template</returns>
        TemplateModel Render(ErrorModel error, ContentInfo details);

    }

}
