using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Model used by templates to render the actual HTML returned
    /// to the client.
    /// </summary>
    public class TemplateModel : IBaseModel
    {

        #region Get-/Setters

        /// <summary>
        /// The title to be rendered within the template.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The description to be rendered within the template.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// The HTML content to be rendered within the template.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The request which caused this call.
        /// </summary>
        public IRequest Request { get; }

        /// <summary>
        /// The handler responsible to render the response.
        /// </summary>
        public IHandler Handler { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new model instance.
        /// </summary>
        /// <param name="request">The request which caused this call</param>
        /// <param name="handler">The handler responsible to render the response</param>
        /// <param name="title">The title to be rendered within the template</param>
        /// <param name="description">The description to be rendered within the template</param>
        /// <param name="content">The content to be rendered within the template</param>
        public TemplateModel(IRequest request, IHandler handler, string title, string? description, string content)
        {
            Title = title;
            Description = description;
            Content = content;

            Request = request;
            Handler = handler;
        }

        #endregion

    }

}

