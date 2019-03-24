using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol.Exceptions
{

    /// <summary>
    /// This exception will occur, whenever you try to sent data over a HttpResponse which
    /// has already been used to send data.
    /// </summary>
    public class ResponseAlreadySentException : Exception
    {

        /// <summary>
        /// Create a new exception of this type.
        /// </summary>
        public ResponseAlreadySentException() : base("This HttpResponse has already been used to send data.")
        {

        }

    }

}
