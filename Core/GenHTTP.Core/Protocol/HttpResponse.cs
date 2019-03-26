using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Exceptions;

namespace GenHTTP.Core
{

    /// <summary>
    /// Represents a HTTP response.
    /// </summary>
    [Serializable]
    public class HttpResponse : IHttpResponse
    {
        private HttpResponseHeader _Header;
        private ClientHandler _Handler;

        private ulong? _ContentLength;

        private bool _IsHead = false;
        private bool _Sent = false;

        #region Initialization

        /// <summary>
        /// Create a new HTTP response.
        /// </summary>
        /// <param name="handler">The handler relating to the response</param>
        /// <param name="isHead">Specify, whether this is an answer on a HTTP HEAD request or not</param>
        /// <param name="protocolType">The protocol type the client acceppts</param>
        /// <param name="keepAlive">Specify, whether the connection should keep alive</param>
        internal HttpResponse(ClientHandler handler, bool isHead, ProtocolType protocolType, bool keepAlive)
        {
            _IsHead = isHead;
            _Handler = handler;
            _Header = new HttpResponseHeader(this, protocolType, keepAlive);
        }

        #endregion

        #region Get-/Setters

        /// <summary>
        /// The handler the response should be written to.
        /// </summary>
        public IClientHandler ClientHandler
        {
            get
            {
                return _Handler;
            }
        }

        /// <summary>
        /// The HTTP response header.
        /// </summary>
        public IHttpResponseHeader Header
        {
            get
            {
                return _Header;
            }
        }

        /// <summary>
        /// The content length of the sent (!) response.
        /// </summary>
        public ulong? ContentLenght
        {
            get
            {
                return _ContentLength;
            }
        }

        /// <summary>
        /// Check, whether the response has already been used to send data.
        /// </summary>
        public bool Sent
        {
            get { return _Sent; }
        }

        /// <summary>
        /// Specifies, whether this is a response on a HEAD request.
        /// </summary>
        public bool IsHead
        {
            get { return _IsHead; }
        }

        #endregion

        #region Functionality

        public void Send(Stream content)
        {
            if (_Sent)
            {
                throw new ResponseAlreadySentException();
            }

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }

            var length = (content.Length > 0) ? (ulong?)content.Length : null;

            _Header.WriteHeader(_Handler, length);

            if (!IsHead)
            {
                _Handler.Send(content);
            }

            _ContentLength = length;
            _Sent = true;
        }
        
        public void Send(byte[] content)
        {
            using (var stream = new MemoryStream(content))
            {
                Send(stream);
            }
        }

        #endregion

    }

}
