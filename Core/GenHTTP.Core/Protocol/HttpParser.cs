using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Protocol;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core
{

    /// <summary>
    /// Watches the connection and parses HttpRequests.
    /// </summary>
    internal class HttpParser
    {
        private byte[] _SocketBuffer = new byte[4096];

        #region Get-/Setters

        protected bool KeepAlive { get; set; }

        protected bool FirstConnection { get; set; }

        protected ClientHandler Handler { get; }

        protected Socket Socket { get; }

        protected Server Server { get; }

        protected HttpRequest CurrentRequest { get; set; }

        protected HttpScanner Scanner { get; set; }

        protected ushort Timeout { get; }

        protected string? CurrentHeader { get; set; }

        protected bool BodyAvailable { get; set; }

        protected uint RequestsPending { get; set; }

        protected DateTime LastRequest { get; set; }

        protected bool TimedOut
        {
            get
            {
                if (RequestsPending > 0)
                {
                    return false;
                }

                return (DateTime.Now - LastRequest).TotalSeconds > Timeout;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Create a new HttpParser object.
        /// </summary>
        /// <param name="socket">The connection to watch</param>
        /// <param name="handler">The assigned client handler</param>
        internal HttpParser(Socket socket, Server server, ClientHandler handler)
        {
            Handler = handler;
            Socket = socket;
            Server = server;

            Timeout = 5;

            FirstConnection = true;
            KeepAlive = false;

            LastRequest = DateTime.Now;
            BodyAvailable = false;

            Scanner = new HttpScanner();
            CurrentRequest = new HttpRequest(Handler);
        }

        #endregion

        #region Functionality

        internal void Run()
        {
            try
            {
                // begin to retrieve data
                Socket.BeginReceive(_SocketBuffer, 0, _SocketBuffer.Length, SocketFlags.None, new AsyncCallback(RecievedData), null);

                // block this thread, waiting for timeout
                while (Socket.Connected && !TimedOut) Thread.Sleep(10);
            }
            finally
            {
                if (Socket.Connected)
                {
                    Socket.Disconnect(false);
                    Socket.Close();
                }
            }
        }

        private void RecievedData(IAsyncResult result)
        {
            try
            {
                // retrieve data
                int read = Socket.EndReceive(result);

                if (read == 0)
                {
                    return;
                }

                Scanner.Append(Encoding.ASCII.GetString(_SocketBuffer, 0, read));

                // parse it
                while (Scanner.NextToken() != HttpToken.Unknown)
                {
                    Parse();
                }

                // and enqueue again
                Socket.BeginReceive(_SocketBuffer, 0, _SocketBuffer.Length, SocketFlags.None, new AsyncCallback(RecievedData), null);
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
            }
        }

        private void Parse()
        {
            try
            {
                // GET, HEAD or POST
                if (Scanner.Current == HttpToken.Method)
                {
                    LastRequest = DateTime.Now; // reset timeout-timer
                    CurrentRequest.ParseType(Scanner.Value);
                    return;
                }

                // HTTP version
                if (Scanner.Current == HttpToken.Http)
                {
                    CurrentRequest.ParseHttp(Scanner.Value);
                    return;
                }

                // URL
                if (Scanner.Current == HttpToken.Url)
                {
                    CurrentRequest.ParseURL(Scanner.Value);
                    return;
                }

                // Save header field
                if (Scanner.Current == HttpToken.HeaderDefinition)
                {
                    CurrentHeader = Scanner.Value;
                    return;
                }

                // Value of a header field
                if (Scanner.Current == HttpToken.HeaderContent)
                {
                    if (CurrentHeader != null)
                    {
                        if (CurrentHeader.ToLower() == "content-length")
                        {
                            Scanner.SetContentLength(Convert.ToInt64(Scanner.Value));
                            BodyAvailable = true;
                        }

                        CurrentRequest.ParseHeaderField(CurrentHeader, Scanner.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException("Header content expected.");
                    }

                    return;
                }

                // new line, check for content
                if (Scanner.Current == HttpToken.NewLine)
                {
                    if (BodyAvailable)
                    {
                        // there is some content, scan it next time
                        Scanner.UseContentPattern = true;
                        BodyAvailable = false;
                    }
                    else
                    {
                        // there is no content ... request complete
                        HandleRequest();
                    }
                }

                // content
                if (Scanner.Current == HttpToken.Content)
                {
                    CurrentRequest.ParseBody(Scanner.Value);
                    HandleRequest();
                }
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.Parser, e);
            }
        }

        private void HandleRequest()
        {
            RequestsPending++;

            var connection = (CurrentRequest["connection"] ?? "").ToLower();

            if (FirstConnection)
            {
                FirstConnection = false;
                KeepAlive = (connection == "keep-alive");
            }
            else
            {
                if (connection == "close")
                {
                    KeepAlive = false;
                }
            }

            try
            {
                var requestHandler = new RequestHandler(Server, Handler);

                if (requestHandler.HandleRequest(CurrentRequest, KeepAlive))
                {
                    Socket.LingerState = new LingerOption(true, 1);
                    Socket.Disconnect(false);
                    Socket.Close();
                }
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.Internal, e);
            }
            finally
            {
                RequestsPending--;
            }

            CurrentRequest = new HttpRequest(Handler);
        }

        #endregion

    }


}
