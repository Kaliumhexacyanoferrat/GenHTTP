using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Model for pages which can be served by an application
    /// to the clients.
    /// </summary>
    public abstract class AbstractModel : IModel
    {

        #region Get-/Setters

        /// <summary>
        /// The request which caused this rendering call.
        /// </summary>
        public IRequest Request { get; }

        /// <summary>
        /// The handler responsible to render the response.
        /// </summary>
        public IHandler Handler { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new page model for the given request.
        /// </summary>
        /// <param name="request">The request which caused this rendering call</param>
        /// <param name="handler">The handler responsible to render the response</param>
        protected AbstractModel(IRequest request, IHandler handler)
        {
            Request = request;
            Handler = handler;
        }

        #endregion

        #region Functionality

        public abstract ValueTask<ulong> CalculateChecksumAsync();

        #endregion

    }

}
