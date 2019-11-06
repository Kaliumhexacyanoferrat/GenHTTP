using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Allow to execute logic for every request handled by the server,
    /// allowing to add custom behavior.
    /// </summary>
    public interface IServerExtension
    {

        /// <summary>
        /// Will be invoked after the request has been received from the client
        /// and the routing context has been determined. If this method returns
        /// a response, the server will bypass the application logic and render
        /// the given content to the client.
        /// </summary>
        /// <param name="request">The request received from the client</param>
        /// <returns>A response, if the extension decides to intercept this request</returns>
        IContentProvider? Intercept(IRequest request);

        /// <summary>
        /// Will be invoked, before the server will send the given response
        /// to the client. The response may be modified to the needs of your
        /// extension.
        /// </summary>
        /// <param name="request">The request which has been handled</param>
        /// <param name="response">The response which should be send to the client</param>
        /// <remarks>
        /// You may change any property of the given response, but you
        /// need to ensure, that your modifications match the semantics
        /// of the HTTP protocol. Otherwise, the server may throw an
        /// error when processing the response.
        /// </remarks>
        void Intercept(IRequest request, IResponse response);

    }

}
