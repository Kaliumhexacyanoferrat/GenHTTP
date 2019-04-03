
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Will be thrown, if the server cannot bind to the requested port for some reason.
    /// </summary>
    public class BindingException : Exception
    {

        public BindingException(string message, Exception inner) : base(message, inner)
        {

        }

    }

}
