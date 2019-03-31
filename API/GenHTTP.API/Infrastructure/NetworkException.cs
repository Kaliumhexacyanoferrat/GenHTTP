using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public class NetworkException : Exception
    {
        
        public NetworkException(string reason, Exception? inner = null) : base(reason, inner)
        {

        }

    }

}
