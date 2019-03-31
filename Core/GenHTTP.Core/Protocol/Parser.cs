using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    internal class Parser
    {
        private byte[] _SocketBuffer = new byte[4096];

        #region Get-/Setters
                
        protected Socket Socket { get; }
        
        protected IServer Server { get; }

        protected Action<RequestBuilder> RequestHandler { get; }

        protected RequestBuilder CurrentRequest { get; set; }

        protected Scanner Scanner { get; set; }

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

        internal Parser(Socket socket, IServer server, Action<RequestBuilder> requestHandler)
        {
            Socket = socket;
            Server = server;

            Timeout = 5;

            LastRequest = DateTime.Now;
            BodyAvailable = false;

            Scanner = new Scanner();
            CurrentRequest = new RequestBuilder();

            RequestHandler = requestHandler;
        }

        #endregion

        #region Functionality

        internal void Run()
        {
            // begin to retrieve data
            Socket.BeginReceive(_SocketBuffer, 0, _SocketBuffer.Length, SocketFlags.None, new AsyncCallback(RecievedData), null);

            // block this thread, waiting for timeout
            while (Socket.Connected && !TimedOut) Thread.Sleep(10);
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
                while (Scanner.NextToken() != Token.Unknown)
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
                if (Scanner.Current == Token.Method)
                {
                    LastRequest = DateTime.Now; // reset timeout-timer
                    CurrentRequest.Type(Scanner.Value);
                    return;
                }

                // HTTP version
                if (Scanner.Current == Token.Http)
                {
                    CurrentRequest.Protocol(Scanner.Value);
                    return;
                }

                // URL
                if (Scanner.Current == Token.Url)
                {
                    CurrentRequest.Path(Scanner.Value);
                    return;
                }

                // Save header field
                if (Scanner.Current == Token.HeaderDefinition)
                {
                    CurrentHeader = Scanner.Value;
                    return;
                }

                // Value of a header field
                if (Scanner.Current == Token.HeaderContent)
                {
                    if (CurrentHeader != null)
                    {
                        if (CurrentHeader.ToLower() == "content-length")
                        {
                            Scanner.SetContentLength(Convert.ToInt64(Scanner.Value));
                            BodyAvailable = true;
                        }

                        CurrentRequest.Header(CurrentHeader, Scanner.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException("Header content expected.");
                    }

                    return;
                }

                // new line, check for content
                if (Scanner.Current == Token.NewLine)
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
                if (Scanner.Current == Token.Content)
                {
                    var bytes = Encoding.UTF8.GetBytes(Scanner.Value);
                    var stream = new MemoryStream(bytes);

                    CurrentRequest.Content(stream);
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

            try
            {
                RequestHandler(CurrentRequest);
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.Internal, e);
            }

            RequestsPending--;

            CurrentRequest = new RequestBuilder();
        }

        #endregion

    }


}
