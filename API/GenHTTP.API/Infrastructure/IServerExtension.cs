using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
