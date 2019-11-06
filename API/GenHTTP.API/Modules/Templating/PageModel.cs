using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{

    /// <summary>
    /// Model for pages which can be served by an application
    /// to the clients.
    /// </summary>
    public class PageModel : IBaseModel
    {

        #region Get-/Setters

        /// <summary>
        /// The title of the page to be rendered.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The request which caused this rendering call.
        /// </summary>
        public IRequest Request { get; }

        #endregion

        #region Functionality

        /// <summary>
        /// Creates a new page model for the given request.
        /// </summary>
        /// <param name="request">The request which caused this rendering call</param>
        public PageModel(IRequest request)
        {
            Request = request;
        }

        #endregion

    }

}
