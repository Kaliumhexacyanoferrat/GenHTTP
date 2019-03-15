using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP
{

    /// <summary>
    /// Signature of a function which will be called after a request has been handled by a project.
    /// </summary>
    /// <param name="request">The request sent to the server</param>
    /// <param name="response">The response of the project</param>
    public delegate void RequestHandled(HttpRequest request, HttpResponse response);

    /// <summary>
    /// Signature of the method which will be called if the timer ticks.
    /// </summary>
    public delegate void TimerTick();

}
