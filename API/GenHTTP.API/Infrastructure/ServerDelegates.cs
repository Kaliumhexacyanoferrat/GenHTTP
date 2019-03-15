using GenHTTP.Api.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Signature of a function which will be called after a request has been handled by a project.
    /// </summary>
    /// <param name="request">The request sent to the server</param>
    /// <param name="response">The response of the project</param>
    public delegate void RequestHandled(IHttpRequest request, IHttpResponse response);

    /// <summary>
    /// Signature of the method which will be called if the timer ticks.
    /// </summary>
    public delegate void TimerTick();

}
