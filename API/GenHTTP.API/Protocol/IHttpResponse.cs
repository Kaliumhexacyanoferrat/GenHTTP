using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    public interface IHttpResponse
    {
        
        /// <summary>
        /// The HTTP response header.
        /// </summary>
        IHttpResponseHeader Header { get; }

        /// <summary>
        /// The content length of the sent (!) response.
        /// </summary>
        ulong? ContentLenght { get; }

        /// <summary>
        /// The time needed to respond.
        /// </summary>
        TimeSpan? LoadTime { get; }

        /// <summary>
        /// Check, whether the response has already been used to send data.
        /// </summary>
        bool Sent { get; }

        /// <summary>
        /// The handler the response should be written to.
        /// </summary>
        IClientHandler ClientHandler { get; }

        void Send(Stream content);

    }

}
