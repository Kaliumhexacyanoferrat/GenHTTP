using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{

    /// <summary>
    /// Defines how session references are sent to the client
    /// and read from the incoming requests.
    /// </summary>
    public interface ISessionHandling
    {

        /// <summary>
        /// Attempts to read a session token from the given request.
        /// </summary>
        /// <param name="request">The request to be analyzed</param>
        /// <returns>The session token read from the request (if present)</returns>
        string? ReadToken(IRequest request);

        /// <summary>
        /// Modifies the given response to instruct the client to use the
        /// given session token.
        /// </summary>
        /// <param name="response">The response to be modified</param>
        /// <param name="sessionToken">The token to be written to the client</param>
        void WriteToken(IResponse response, string sessionToken);

        /// <summary>
        /// Configures the response to instruct the client that the
        /// session token is no longer valid and should be discarded.
        /// </summary>
        /// <param name="response">The response to be modified</param>
        void ClearToken(IResponse response);

    }

}
