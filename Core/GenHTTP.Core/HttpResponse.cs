using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Caching;
using GenHTTP.Abstraction;
using GenHTTP.Abstraction.Style;
using GenHTTP.Abstraction.Compiling;

namespace GenHTTP
{

    /// <summary>
    /// Represents a HTTP response.
    /// </summary>
    [Serializable]
    public class HttpResponse
    {
        private HttpResponseHeader _Header;
        private ClientHandler _Handler;
        private ulong _ContentLength;
        private bool _UseCompression;
        private bool _IsHead = false;
        private bool _Sent = false;

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
            // congestion control
            if (handler.Server.Congested)
            {
                _Header.CloseConnection = true;
            }
        }

        #region get-/setters

        /// <summary>
        /// Specifies, whether the response should be sent compressed.
        /// </summary>
        /// <remarks>
        /// The client handler will set this value on initialization. Set this value to true only,
        /// if the client supports compression (<see cref="HttpRequest.CompressionAvailable" />).
        /// 
        /// This class will only compress plain text content types (html, css, js, ...).
        /// </remarks>
        public bool UseCompression
        {
            get { return _UseCompression; }
            set { _UseCompression = value; }
        }

        /// <summary>
        /// The handler the response should be written to.
        /// </summary>
        public ClientHandler ClientHandler
        {
            get
            {
                return _Handler;
            }
        }

        /// <summary>
        /// The HTTP response header.
        /// </summary>
        public HttpResponseHeader Header
        {
            get
            {
                return _Header;
            }
        }

        /// <summary>
        /// The content length of the sent (!) response.
        /// </summary>
        public ulong ContentLenght
        {
            get
            {
                return _ContentLength;
            }
        }

        /// <summary>
        /// The number of seconds needed to respond
        /// </summary>
        public double LoadTime
        {
            get
            {
                return _Handler.LoadTime;
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


        /// <summary>
        /// Send a (X)HTML document to the client.
        /// </summary>
        /// <param name="document">The document to send</param>
        public void Send(Document document)
        {
            if (_Sent) throw new ResponseAlreadySentException();
            _Header.ContentEncoding = document.Encoding.Encoding;
            Send(document.Serialize());
        }

        /// <summary>
        /// Send a pre-compiled (X)HTML document.
        /// </summary>
        /// <param name="document">The document to send</param>
        public void Send(ITemplate document)
        {
            if (_Sent) throw new ResponseAlreadySentException();
            _Header.ContentEncoding = document.Base.Encoding;
            Send(document.ToByteArray());
        }

        /// <summary>
        /// Send a cached response to the client.
        /// </summary>
        /// <param name="cachedResponse">The response to send</param>
        public void Send(CachedResponse cachedResponse)
        {
            if (_Sent) throw new ResponseAlreadySentException();
            cachedResponse.PrepareResponse(this);
            byte[] buffer = (UseCompression && IsContentCompressable(cachedResponse.Data.Length)) ? cachedResponse.CompressedData : cachedResponse.Data;
            _Header.WriteHeader(_Handler, (ulong)buffer.Length);
            _Handler.SendBytes(buffer);
            _ContentLength = (ulong)buffer.Length;
            _Sent = true;
        }

        /// <summary>
        /// Send a file to the client.
        /// </summary>
        /// <param name="download">The file to send</param>
        public void Send(Download download)
        {
            if (_Sent) throw new ResponseAlreadySentException();
            download.SendFile(_Handler, this);
            _ContentLength = (ulong)download.Length;
            _Sent = true;
        }

        /// <summary>
        /// Send the content of a StringBuilder to the client.
        /// </summary>
        /// <param name="builder">The StringBuilder to read from</param>
        public void Send(StringBuilder builder)
        {
            Send(builder.ToString());
        }

        /// <summary>
        /// Send an UTF-8 encoded string to the client.
        /// </summary>
        /// <param name="text">The string to send</param>
        public void Send(string text)
        {
            if (_Sent) throw new ResponseAlreadySentException();
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            Send(buffer);
        }

        /// <summary>
        /// Send some byes to the client.
        /// </summary>
        /// <param name="buffer">The bytes to send</param>
        public void Send(byte[] buffer)
        {
            if (_Sent) throw new ResponseAlreadySentException();
            if (UseCompression && IsContentCompressable(buffer.LongLength)) buffer = GzipCompression.Compress(buffer);
            _Header.WriteHeader(_Handler, (ulong)buffer.Length);
            if (!IsHead) _Handler.SendBytes(buffer);
            _ContentLength = (ulong)buffer.Length;
            _Sent = true;
        }

        /// <summary>
        /// Defines, whether it makes sense to compress the content of the file, or not.
        /// </summary>
        /// <param name="uncompressedLenght">The length of the data to send</param>
        /// <remarks>
        /// Using an compression algorithm is very useful if there is plain text to send. Images
        /// and other formats (videos, binaries) are already compressed.
        /// 
        /// Large files won't get compressed because they need to be compressed before the data is sent
        /// to the client (which requires a lot of ram on big files). The size limit can be set in the server configuration file.
        /// </remarks>
        public bool IsContentCompressable(long uncompressedLenght)
        {
            if (uncompressedLenght > _Handler.Server.Configuration.CompressionLimit) return false;
            if (_Header.ContentType == ContentType.TextCss) return true;
            if (_Header.ContentType == ContentType.TextHtml) return true;
            if (_Header.ContentType == ContentType.ApplicationJavaScript) return true;
            if (_Header.ContentType == ContentType.TextCsv) return true;
            if (_Header.ContentType == ContentType.TextPlain) return true;
            if (_Header.ContentType == ContentType.TextRichText) return true;
            if (_Header.ContentType == ContentType.TextXml) return true;
            if (_Header.ContentType == ContentType.AudioWav) return true;
            if (_Header.ContentType == ContentType.ApplicationOfficeDocumentPresentation) return true;
            if (_Header.ContentType == ContentType.ApplicationOfficeDocumentSheet) return true;
            if (_Header.ContentType == ContentType.ApplicationOfficeDocumentSlideshow) return true;
            if (_Header.ContentType == ContentType.ApplicationOfficeDocumentWordProcessing) return true;
            return false;
        }

    }

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
