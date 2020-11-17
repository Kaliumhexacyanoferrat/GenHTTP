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
        /// Additional information about the page to be rendered.
        /// </summary>
        public ContentInfo Meta { get; }

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
        /// <param name="pageInfo">Information about the page to be rendered</param>
        /// <param name="content">The content to be rendered within the template</param>
        public TemplateModel(IRequest request, IHandler handler, ContentInfo pageInfo, string content)
        {
            Content = content;

            Meta = pageInfo;

            if (string.IsNullOrEmpty(Meta.Title))
            {
                Meta = Meta with { Title = "Untitled Page" };
            }

            Request = request;
            Handler = handler;
        }

        #endregion

    }

}

