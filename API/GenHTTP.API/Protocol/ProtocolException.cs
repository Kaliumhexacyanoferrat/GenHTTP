using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Thrown by the server, if the HTTP protocol has
    /// somehow been violated (either by the server or the client).
    /// </summary>
    public class ProtocolException : Exception
    {

        public ProtocolException(string reason) : base(reason)
        {

        }

    }

}
