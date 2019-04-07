using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Thrown if a network-level exception occurs.
    /// </summary>
    public class NetworkException : Exception
    {
        
        public NetworkException(string reason, Exception? inner = null) : base(reason, inner)
        {

        }

    }

}
