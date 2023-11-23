using System;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Thrown by the server, if the HTTP protocol has
    /// somehow been violated (either by the server or the client).
    /// </summary>
    [Serializable]
    public class ProtocolException : Exception
    {

        public ProtocolException(string reason) : base(reason)
        {

        }

        public ProtocolException(string reason, Exception inner) : base(reason, inner)
        {

        }

    }

}
