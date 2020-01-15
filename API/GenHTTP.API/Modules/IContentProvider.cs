using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules
{

    /// <summary>
    /// Generates the response which should be sent to the client
    /// for a given request. 
    /// </summary>
    /// <remarks>
    /// Content providers can be registered at routers.
    /// </remarks>
    public interface IContentProvider : IContentDescription
    {

        /// <summary>
        /// Handles the given request, generating the response
        /// to be send to the client.
        /// </summary>
        /// <param name="request">The request to be handled</param>
        /// <returns>The response which should be send to the requesting client</returns>
        IResponseBuilder Handle(IRequest request);

    }

}
