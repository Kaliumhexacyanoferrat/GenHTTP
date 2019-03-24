using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    #region Delegates

    /// <summary>
    /// Signature of a function which will be called after a request has been handled by a project.
    /// </summary>
    /// <param name="request">The request sent to the server</param>
    /// <param name="response">The response of the project</param>
    public delegate void RequestHandled(IHttpRequest request, IHttpResponse response);
    
    #endregion

    public interface IServer : IDisposable
    {

        /// <summary>
        /// You can subscribe to this event if you want to get notified whenever a request was sucessfully handled by a project.
        /// </summary>
        event RequestHandled OnRequestHandled;
        
        /// <summary>
        /// The version of the server software.
        /// </summary>
        Version Version { get; }

        IRouter Router { get; }
          
    }

}
