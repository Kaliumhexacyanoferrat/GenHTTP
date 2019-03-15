using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using GenHTTP.Style;
using GenHTTP.Controls;
using GenHTTP.Utilities;

namespace GenHTTP
{

    /// <summary>
    /// Handles the requests from a browser.
    /// </summary>
    [Serializable]
    public class ClientHandler : MarshalByRefObject
    {
        private Socket _Connection;
        private Server _Server;
        private double _LoadTime;

        /// <summary>
        /// Create a new instance to handle a request from a browser.
        /// </summary>
        /// <param name="socket">The socket to read from</param>
        /// <param name="server">The server this handler relates to</param>
        public ClientHandler(Socket socket, Server server)
        {
            _Connection = socket;
            _Server = server;
        }

        #region get-/setter

        /// <summary>
        /// The server this handler relates to.
        /// </summary>
        public Server Server
        {
            get
            {
                return _Server;
            }
        }

        /// <summary>
        /// Time span between handling the request and sending the response.
        /// </summary>
        public double LoadTime
        {
            get
            {
                return _LoadTime;
            }
        }

        /// <summary>
        /// The IP of the connected client.
        /// </summary>
        public string IP
        {
            get
            {
                return ((IPEndPoint)_Connection.RemoteEndPoint).Address.ToString();
            }
        }

        /// <summary>
        /// The port of the connected client.
        /// </summary>
        public int Port
        {
            get
            {
                return ((IPEndPoint)_Connection.RemoteEndPoint).Port;
            }
        }

        #endregion

        /// <summary>
        /// Begin to handle the client's requests.
        /// </summary>
        internal void Run(object state)
        {
            try
            {
                // run parser
                HttpParser parser = new HttpParser(_Connection, this);
                parser.Run();
            }
            catch { }
        }

        internal bool HandleRequest(HttpRequest request, bool keepAlive)
        {
            // determine protocol to use
            ProtocolType type = _Server.Configuration.Protocol;
            // handle the request
            if (request != null)
            {
                // enable virtual hosting, following the rules from the server's config file
                if (request.Host != null && _Server.Configuration.VHostsEnabled)
                {
                    foreach (KeyValuePair<string, string> vhost in _Server.Configuration.GetVHosts())
                    {
                        // check, whether this rule matches this request
                        if (request.Host == vhost.Key)
                        {
                            try
                            {
                                // redirect the request to the selected project
                                request.Redirect("/" + vhost.Value + request.File);
                                // change the project of the request
                                request.Project = _Server.Projects[vhost.Value];
                                request.VirtualHosting = true;
                            }
                            catch (Exception e)
                            {
                                // failed to redirect request
                                _Server.Log.WriteLineColored("Failed to redirect request '" + request.File + "': " + e.Message, ConsoleColor.Red);
                            }
                        }
                    }
                }
                // retrieve the responsible project for this request
                if (request.Project == null)
                {
                    HttpResponse response = new HttpResponse(this, request.Type == RequestType.HEAD, type, keepAlive);
                    // no project is responsible for this project
                    // let the helper decide what to do
                    try
                    {
                        if (request.File == "/")
                        {
                            // index requested
                            _Server.Helper.GenerateIndex(request, response);
                        }
                        else
                        {
                            if (!_Server.Helper.GenerateOther(request, response))
                            {
                                if (!response.Sent) _Server.Helper.GenerateNotFound(request, response);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _Server.Log.WriteLine("Helper failed to handle request '" + request.File + "': " + e.Message);
                    }
                    // did the handler send any content?
                    if (!response.Sent)
                    {
                        Send500(request, response);
                    }
                    else
                    {
                        // call completion event
                        _Server.CallCompletionEvent(request, response);
                    }
                }
                else
                {
                    HttpResponse response = new HttpResponse(this, request.Type == RequestType.HEAD, type, keepAlive);
                    try
                    {
                        DateTime before = DateTime.Now;
                        if (request.CompressionAvailable) response.UseCompression = true; else response.UseCompression = false;
                        if (!_Server.Configuration.CompressionEnabled || _Server.Configuration.CompressionAlgorithm != "gzip") response.UseCompression = false;
                        request.Project.HandleRequest(request, response);
                        _LoadTime = (DateTime.Now - before).TotalSeconds;
                        // log request
                        LogRequest(request, response);
                        // call completion event
                        //_Server.CallCompletionEvent(request, response);
                    }
                    catch (Exception e)
                    {
                        // project failed to handle this request
                        if (!response.Sent)
                        {
                            try
                            {
                                _Server.Helper.GenerateServerError(request, response, e);
                            }
                            catch (Exception ex)
                            {
                                // helper failed to help us!
                                _Server.Log.WriteLine("Helper failed to handle project exception (" + e.Message + "), request '" + request.File + "': " + ex.Message);
                            }
                            // did the helper send any content?
                            if (!response.Sent)
                            {
                                Send500(request, response);
                            }
                            else
                            {
                                // call completion event
                                _Server.CallCompletionEvent(request, response);
                            }
                        }
                    }
                    // close connection if requested
                    if (response.Header.CloseConnection) return true;
                }
            }
            else
            {
                _Server.Log.WriteLine("Malformed request from client");
                HttpResponse response = new HttpResponse(this, false, ProtocolType.Http_1_0, keepAlive);
                try
                {
                    _Server.Helper.GenerateBadRequest(response);
                }
                catch (Exception e)
                {
                    // helper failed to help us
                    _Server.Log.WriteLine("Helper failed to send bad request page: " + e.Message);
                }
                // did the helper send any content?
                if (!response.Sent)
                {
                    Send500(request, response);
                }
                else
                {
                    // call completion event
                    _Server.CallCompletionEvent(request, response);
                }
            }
            // client requests to close the connection
            if (!keepAlive) return true;
            // return, whether the connection should be left open or not
            return Server.Congested;
        }

        /// <summary>
        /// Send an error page if something went wrong.
        /// </summary>
        /// <param name="response">The response to write to</param>
        /// <param name="request">The request which is responsible for this error</param>
        private void Send500(HttpRequest request, HttpResponse response)
        {
            response.Header.Type = ResponseType.InternalServerError;
            response.Header.ContentType = ContentType.TextPlain;
            response.Send("500 - Internal Server Error. Please contact the administrator of this server.");
            // call completion event
            _Server.CallCompletionEvent(request, response);
        }

        private void LogRequest(HttpRequest request, HttpResponse response)
        {
            lock (_Server.Log)
            {
                _Server.Log.WriteTimestamp();
                _Server.Log.WriteColored(response.ClientHandler.IP.PadRight(14, ' '), ConsoleColor.Yellow);
                string status = response.Header.GetStatusCode(response.Header.Type);
                if (status.StartsWith("4") || status.StartsWith("5"))
                {
                    _Server.Log.WriteColored(" " + response.Header.GetStatusCode(response.Header.Type), ConsoleColor.Red);
                }
                else
                {
                    _Server.Log.WriteColored(" " + response.Header.GetStatusCode(response.Header.Type), ConsoleColor.Green);
                }
                _Server.Log.WriteColored(" " + HttpRequest.GetRequestTypeName(request.Type), ConsoleColor.White);
                _Server.Log.WriteColored(" " + request.File, ConsoleColor.Gray);
                _Server.Log.WriteRightAlign(response.ContentLenght.ToString().PadLeft(5, ' '), ConsoleColor.DarkMagenta);
            }
        }

        private void SendData(string data)
        {
            _Connection.Send(Encoding.ASCII.GetBytes(data));
        }

        /// <summary>
        /// Write some data to the socket.
        /// </summary>
        /// <param name="tosend">The data to send</param>
        internal void SendBytes(byte[] tosend)
        {
            try
            {
                _Connection.Send(tosend);

                //_Connection.BeginSend(tosend, 0, tosend.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch { }
        }

        private void SendCallback(IAsyncResult result)
        {
            _Connection.EndSend(result);
        }

    }

}
