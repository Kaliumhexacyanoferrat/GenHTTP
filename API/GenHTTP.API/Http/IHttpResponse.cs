using GenHTTP.Api.Caching;
using GenHTTP.Api.Compilation;
using GenHTTP.Api.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using GenHTTP.Api.Content;

namespace GenHTTP.Api.Http
{

    public interface IHttpResponse
    {
        
        /// <summary>
        /// The HTTP response header.
        /// </summary>
        IHttpResponseHeader Header { get; }

        /// <summary>
        /// Specifies, whether the response should be sent compressed.
        /// </summary>
        bool UseCompression { get; }

        /// <summary>
        /// The content length of the sent (!) response.
        /// </summary>
        ulong ContentLenght { get; }

        /// <summary>
        /// The number of seconds needed to respond
        /// </summary>
        double LoadTime { get; }

        /// <summary>
        /// Check, whether the response has already been used to send data.
        /// </summary>
        bool Sent { get; }

        /// <summary>
        /// Specifies, whether this is a response on a HEAD request.
        /// </summary>
        bool IsHead { get; }

        /// <summary>
        /// The handler the response should be written to.
        /// </summary>
        IClientHandler ClientHandler { get; }

        /// <summary>
        /// Send a (X)HTML document to the client.
        /// </summary>
        /// <param name="document">The document to send</param>
        void Send(Document document);

        /// <summary>
        /// Send a pre-compiled (X)HTML document.
        /// </summary>
        /// <param name="document">The document to send</param>
        void Send(ITemplate document);

        /// <summary>
        /// Send a cached response to the client.
        /// </summary>
        /// <param name="cachedResponse">The response to send</param>
        void Send(CachedResponse cachedResponse);

        /// <summary>
        /// Send a file to the client.
        /// </summary>
        /// <param name="download">The file to send</param>
        void Send(Download download);

        /// <summary>
        /// Send the content of a StringBuilder to the client.
        /// </summary>
        /// <param name="builder">The StringBuilder to read from</param>
        void Send(StringBuilder builder);

        /// <summary>
        /// Send an UTF-8 encoded string to the client.
        /// </summary>
        /// <param name="text">The string to send</param>
        void Send(string text);

        /// <summary>
        /// Send some byes to the client.
        /// </summary>
        /// <param name="buffer">The bytes to send</param>
        void Send(byte[] buffer);

    }

}
