using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Content provider that is able to handle a request and return a HTTP
    /// response to it.
    /// </summary>
    public interface IHandler
    {

        /// <summary>
        /// The parent of this handler within the routing tree.
        /// </summary>
        IHandler Parent { get; }

        /// <summary>
        /// Describes the content that is provided by this handler.
        /// </summary>
        /// <param name="request">The request to be respected when describing the content</param>
        /// <returns>The content provided by this handler</returns>
        IEnumerable<ContentElement> GetContent(IRequest request);

        /// <summary>
        /// Handles the given request and returns a response, if applicable.
        /// </summary>
        /// <remarks>
        /// Not returning a response causes the server to respond with a not found
        /// response code.
        /// </remarks>
        /// <param name="request">The request to be handled</param>
        /// <returns>The response to be sent to the requesting client</returns>
        ValueTask<IResponse?> HandleAsync(IRequest request);

    }

}
