using GenHTTP.Api.Http;
using GenHTTP.Api.SessionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Describes methods which must be implemented by content
    /// providing classes.
    /// </summary>
    public interface IContentProvider
    {

        /// <summary>
        /// Specifies, whether this content provider needs a logged in user.
        /// </summary>
        bool RequiresLogin { get; }

        /// <summary>
        /// Check, whether the content provider is responsible for a given request.
        /// </summary>
        /// <param name="request">The request to check the resposibility for</param>
        /// <param name="info">Information about the client's session</param>
        /// <returns>true, if the content provider can handle this request</returns>
        bool IsResponsible(IHttpRequest request, AuthorizationInfo info);

        /// <summary>
        /// Provides content to the client.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="response">The response to write to</param>
        /// <param name="info">Information about the session of the client</param>
        void HandleRequest(IHttpRequest request, IHttpResponse response, AuthorizationInfo info);

    }

}
