using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Core
{

    internal delegate void HandleRequest(HttpRequest request);

    /// <summary>
    /// Watches the connection and parses HttpRequests.
    /// </summary>
    internal class HttpParser
    {
        private ClientHandler _Handler;
        private Socket _Socket;
        private HttpRequest _CurrentRequest;
        private HttpScanner _Scanner;
        private Server _Server;
        private string _CurrentHeader;
        private bool _BodyAvailable;
        private uint _RequestsPending = 0;
        private DateTime _LastRequest;
        private int _BufferSize;
        private ushort _Timeout = 300;
        private bool _FirstConnection = true;
        private bool _KeepAlive = false;

        private byte[] _SocketBuffer;

        /// <summary>
        /// Create a new HttpParser object.
        /// </summary>
        /// <param name="socket">The connection to watch</param>
        /// <param name="handler">The assigned client handler</param>
        internal HttpParser(Socket socket, Server server, ClientHandler handler)
        {
            _Handler = handler;
            _Socket = socket;
            _Server = server;
            _BufferSize = 1500;
            _Timeout = 10;
        }

        /// <summary>
        /// Begin to parse.
        /// </summary>
        /// <remarks>
        /// Abort this function by calling the Abort() method.
        /// </remarks>
        internal void Run()
        {
            _LastRequest = DateTime.Now;
            _BodyAvailable = false;
            _Scanner = new HttpScanner();
            _CurrentRequest = new HttpRequest(_Handler);
            // create new buffer to store retrieved data
            _SocketBuffer = new byte[_BufferSize];
            try
            {
                // begin to retrieve data
                _Socket.BeginReceive(_SocketBuffer, 0, _BufferSize, SocketFlags.None, new AsyncCallback(RecievedData), null);
                // block this thread, waiting for timeout
                while (_Socket.Connected && !Timeout) Thread.Sleep(10);
            }
            finally
            {
                if (_Socket.Connected)
                {
                    _Socket.Disconnect(false);
                    _Socket.Close();
                }
            }
        }

        private bool RefreshConnectionState()
        {
            bool ret = true;
            bool blockingState = _Socket.Blocking;
            try
            {
                byte[] tmp = new byte[1];
                _Socket.Blocking = false;
                _Socket.Send(tmp, 0, 0);
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10035))
                {
                    ret = false;
                }
            }
            finally
            {
                _Socket.Blocking = blockingState;
            }
            return ret;
        }

        private void RecievedData(IAsyncResult result)
        {
            try
            {
                // retrieve data
                int read = _Socket.EndReceive(result);
                if (read == 0) return;
                _Scanner.AddToScan(Encoding.ASCII.GetString(_SocketBuffer, 0, read));
                // parse it
                while (_Scanner.NextToken() != HttpToken.Unknown) Parse();
                // and enqueue again
                _Socket.BeginReceive(_SocketBuffer, 0, _BufferSize, SocketFlags.None, new AsyncCallback(RecievedData), null);
            }
            catch { }
        }

        private void Parse()
        {
            try
            {
                // GET, HEAD or POST
                if (_Scanner.Current == HttpToken.Method)
                {
                    _LastRequest = DateTime.Now; // reset timeout-timer
                    _CurrentRequest.ParseType(_Scanner.Value);
                    return;
                }
                // HTTP version
                if (_Scanner.Current == HttpToken.Http)
                {
                    _CurrentRequest.ParseHttp(_Scanner.Value);
                    return;
                }
                // URL
                if (_Scanner.Current == HttpToken.Url)
                {
                    _CurrentRequest.ParseURL(_Scanner.Value);
                    return;
                }
                // Save header field
                if (_Scanner.Current == HttpToken.HeaderDefinition)
                {
                    _CurrentHeader = _Scanner.Value;
                    return;
                }
                // Value of a header field
                if (_Scanner.Current == HttpToken.HeaderContent)
                {
                    if (_CurrentHeader.ToLower() == "content-length")
                    {
                        _Scanner.SetContentLength(Convert.ToInt64(_Scanner.Value));
                        _BodyAvailable = true;
                    }
                    _CurrentRequest.ParseHeaderField(_CurrentHeader, _Scanner.Value);
                    return;
                }
                // new line, check for content
                if (_Scanner.Current == HttpToken.NewLine)
                {
                    if (_BodyAvailable)
                    {
                        // there is some content, scan it next time
                        _Scanner.UseContentPattern = true;
                        _BodyAvailable = false;
                    }
                    else
                    {
                        // there is no content ... request complete
                        HandleRequest();
                    }
                }
                // content
                if (_Scanner.Current == HttpToken.Content)
                {
                    _CurrentRequest.ParseBody(_Scanner.Value);
                    HandleRequest();
                }
            }
            catch (Exception e)
            {
                var log = _Server.LoggerFactory.CreateLogger<HttpParser>();
                log.LogError(e, $"Failed to parse request from client '{_Handler.IPAddress}'.");
            }
        }

        private void HandleRequest()
        {
            _RequestsPending++;

            var connection = (_CurrentRequest["connection"] ?? "").ToLower();

            if (_FirstConnection)
            {
                _FirstConnection = false;
                _KeepAlive = connection == "keep-alive";
            }
            else
            {
                if (connection == "close") _KeepAlive = false;
            }

            try
            {
                if (_Handler.HandleRequest(_CurrentRequest, _KeepAlive))
                {
                    _Socket.LingerState = new LingerOption(true, 1);
                    _Socket.Disconnect(false);
                    _Socket.Close();
                }
                _RequestsPending--;
            }
            catch { _RequestsPending--; }

            _CurrentRequest = new HttpRequest(_Handler);
        }

        /// <summary>
        /// Check, whether the 
        /// </summary>
        internal bool Timeout
        {
            get
            {
                if (_RequestsPending > 0) return false;
                return (DateTime.Now - _LastRequest).TotalSeconds > _Timeout;
            }
        }

    }


}
