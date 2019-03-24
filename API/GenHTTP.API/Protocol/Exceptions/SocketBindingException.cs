using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol.Exceptions
{

    /// <summary>
    /// Will be thrown, if the server cannot bind to the requested port for some reason.
    /// </summary>
    public class SocketBindingException : Exception
    {

        public SocketBindingException(string message, Exception inner) : base(message, inner)
        {

        }

    }

}
