using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    public class ProtocolException : Exception
    {

        public ProtocolException(string reason) : base(reason)
        {

        }

    }

}
