using System;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Thrown if a network-level exception occurs.
    /// </summary>
    [Serializable]
    public class NetworkException : Exception
    {

        public NetworkException(string reason, Exception? inner = null) : base(reason, inner)
        {

        }

    }

}
