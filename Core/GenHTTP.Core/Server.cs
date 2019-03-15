using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Compilation;
using GenHTTP.Api.Project;
using GenHTTP.Api.Content;
using GenHTTP.Api.Http;
using GenHTTP.Core.Utilities;
using GenHTTP.Core.Content;

namespace GenHTTP.Core
{

    /// <summary>
    /// A small webserver written in C# which allows you to run your
    /// own web applications.
    /// </summary>
    /// <remarks>
    /// If you want to create your own project, you need to create a new
    /// DLL with a class inheriting from the <see cref="AbstractProject"/>
    /// class. Copy your newly created library into a sub folder of the
    /// 'projects' folder of the server to deploy it. You may need to restart
    /// the server to load the new application.
    /// </remarks>
    [Serializable]
    public class Server : IServer
    {
        // Configuration
        private Configuration _Configuration;
        private string _Path;
        private readonly string _Version = "2.0.0-alpha";
        // State
        private bool _Congested = false;
        private bool _Paused = false;
        private bool _Exit = false;
        private Socket _Socket;
        private ProjectCollection _Projects;
        private static Dictionary<int, Server> _Instances;
        // Helpers
        private DateTime _Before;
        private IServerHelper _Helper;
        private Log _Log;
        private ITemplateBase _ServerTemplate;

        #region Events

        /// <summary>
        /// You can subscribe to this event if you want to get notified whenever a request was sucessfully handled by a project.
        /// </summary>
        public event RequestHandled OnRequestHandled;

        /// <summary>
        /// The server will call this method every TimerIntervall seconds.
        /// </summary>
        public event TimerTick OnTimerTick;

        /// <summary>
        /// Allow other applications to analyze requests,
        /// </summary>
        /// <param name="request">The request which got handled</param>
        /// <param name="response">The response which was sent</param>
        internal void CallCompletionEvent(HttpRequest request, HttpResponse response)
        {
            if (request != null && response != null && OnRequestHandled != null) OnRequestHandled(request, response);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new server object using a given configuration file.
        /// </summary>
        /// <param name="configurationFile">The configuration file</param>
        public Server(string configurationFile)
        {
            _Configuration = new Configuration(configurationFile);
            Init();
        }

        /// <summary>
        /// Create a new server object using a given configuration.
        /// </summary>
        /// <param name="config">The configuration to use</param>
        public Server(Configuration config)
        {
            _Configuration = config;
            Init();
        }

        /// <summary>
        /// Create a new server object.
        /// </summary>
        public Server()
        {
            _Configuration = new Configuration(Path + "config/settings.xml");
            Init();
        }

        private void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            _Log = new Log(Path + "logs/" + GenHTTP.Core.Log.DateTimeString + ".log", _Configuration.LogToConsole);
            _ServerTemplate = new ServerPageBase();
        }

        #endregion

        #region Listening and dumping

        /// <summary>
        /// Listen on the given port for incoming connections.
        /// </summary>
        public void Listen()
        {
            // record time
            _Before = DateTime.Now;
            // headline
            _Log.WriteLine("");
            _Log.WriteLine("GenHTTP v" + _Version);
            _Log.WriteLine("");
            // add this server to the collection
            if (_Instances == null) _Instances = new Dictionary<int, Server>();
            if (_Instances.ContainsKey(_Configuration.Port)) _Instances[_Configuration.Port] = this; else _Instances.Add(_Configuration.Port, this);
            // log some interessting server settings
            _Log.WriteLine("Configuration file: './config/settings.xml'");
            _Log.WriteLine("Server backlog: " + _Configuration.Backlog);
            _Log.WriteLine("Server port: " + _Configuration.Port);
            _Log.WriteLine("HTTP version: " + _Configuration.Protocol);
            if (_Configuration.CompressionEnabled)
            {
                _Log.WriteLine("Compression: " + _Configuration.CompressionAlgorithm + " (limit is " + _Configuration.CompressionLimit + ")");
            }
            _Log.WriteLine("Timer intervall: " + _Configuration.TimerIntervall + " seconds");
            // set regex cache
            Regex.CacheSize = _Configuration.StaticRegexCacheSize;
            // initialize the server helper
            try
            {
                string helper = _Configuration.ServerHelper;
                string type = _Configuration.ServerHelperType;
                if (helper.StartsWith("./")) helper = Path + helper.Substring(2);
                if (type != "")
                {
                    // instantiate the given type in the given assembly
                    _Helper = AssemblyHelper.Instantiate<IServerHelper>(helper, type);
                    _Log.WriteLine("Server helper: " + helper + " (" + type + ")");
                }
                else
                {
                    // search for matching classes in the given assembly
                    _Helper = AssemblyHelper.Instantiate<IServerHelper>(helper);
                    _Log.WriteLine("Server helper: " + helper);
                }
                // init the server helper
                _Helper.Init(this);
            }
            catch
            {
                _Log.WriteTimestamp();
                _Log.WriteLineColored("Failed to initialize the given server helper (" + _Configuration.ServerHelper + ", " + _Configuration.ServerHelperType + ")", ConsoleColor.Red);
                return;
            }
            // determine the document root
            if (_Configuration.DocumentRoot.StartsWith("./") || _Configuration.DocumentRoot.StartsWith(@".\"))
            {
                _Configuration.DocumentRoot = Path + _Configuration.DocumentRoot.Substring(2);
            }
            _Log.WriteLine("Document root: '" + _Configuration.DocumentRoot + "'");
            _Log.WriteLine("");
            // load projects
            _Projects = new ProjectCollection(this);
            // add exit event handler to allow dumping of projects
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            // set threading settings
            ThreadPool.SetMaxThreads(_Configuration.MaxThreads, _Configuration.MaxThreads * 2);
            ThreadPool.SetMinThreads(_Configuration.MaxThreads, _Configuration.MaxThreads * 2);
            // start the timer
            Thread timerThread = new Thread(new ThreadStart(TimerThread));
            // print startup time
            _Log.WriteLine("Startup complete. Time needed: " + Math.Round((DateTime.Now - _Before).TotalSeconds, 2) + " seconds");
            _Log.WriteLine("");
            // accept new connections
            while (!_Exit)
            {
                while (_Paused) Thread.Sleep(100);
                AcceptLoop();
            }
            // remove this instance from the collection
            if (_Instances.ContainsKey(_Configuration.Port)) _Instances.Remove(_Configuration.Port);
        }

        private void AcceptLoop()
        {
            if (_Paused) return;
            // init socket
            _Socket = null;
            try
            {
                _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                _Socket.Bind(new IPEndPoint(IPAddress.Any, _Configuration.Port));
                _Socket.Listen(_Configuration.Backlog);
                _Log.WriteLine("Server running on port " + _Configuration.Port + " ...");
                _Log.WriteLine("");
            }
            catch
            {
                _Log.WriteTimestamp();
                _Log.WriteLineColored("Couldn't bind socket to port " + _Configuration.Port + ". Retrying in 10 seconds ...", ConsoleColor.Red);
                Thread.Sleep(10000);
                return;
            }
            // main loop
            try
            {
                do
                {
                    Socket tmp = _Socket.Accept();
                    // congestion control
                    if (_Configuration.CongestionPrevention)
                    {
                        int avail_worker, real_worker, dummy;
                        ThreadPool.GetAvailableThreads(out avail_worker, out dummy);
                        ThreadPool.GetMaxThreads(out real_worker, out dummy);
                        if (avail_worker < real_worker * (1 - _Configuration.CongestionLimit))
                        {
                            _Congested = true;
                            Log.WriteLine("Congestion detected: " + avail_worker + " threads remaining");
                        }
                        else
                        {
                            _Congested = false;
                        }
                    }
                    // handle request
                    ClientHandler handler = new ClientHandler(tmp, this);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Run));
                } while (!_Paused && !_Exit);
            }
            catch { }
        }

        /// <summary>
        /// Dump all projects.
        /// </summary>
        public void Dump()
        {
            _Log.WriteLine("");
            _Log.WriteLine("Dumping ...");
            _Log.WriteLine("");
            // dump the projects
            foreach (IProject project in _Projects)
            {
                try
                {
                    project.Dump();
                    _Log.WriteLine("Dumped project '" + project.Name + "'");
                }
                catch
                {
                    _Log.WriteTimestamp();
                    _Log.WriteLineColored("Could not dump project '" + project.Name + "'", ConsoleColor.Red);
                }
            }
            _Log.WriteLine("");
            _Log.WriteLine("Dump complete.");
            _Log.WriteLine("");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// This method will trigger the timer event.
        /// </summary>
        private void TimerThread()
        {
            DateTime waitTill = DateTime.Now.AddSeconds(_Configuration.TimerIntervall);
            while (!_Exit)
            {
                Thread.Sleep((int)Math.Round(_Configuration.TimerIntervall / 4.0, 0));
                if (DateTime.Now > waitTill)
                {
                    waitTill = DateTime.Now.AddSeconds(_Configuration.TimerIntervall);
                    if (OnTimerTick != null) OnTimerTick();
                }
            }
        }

        /// <summary>
        /// If a assembly can not get resolved by the CLR, we will search the
        /// \extensions\ subdirectory for an assembly with this name.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">Information about the assembly to search for</param>
        /// <returns>The requested assembly</returns>
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int pos = args.Name.IndexOf(","); // not the english way =\ 
            try
            {
                _Log.WriteLine("Providing assembly '" + args.Name.Substring(0, pos) + "'");
                return Assembly.LoadFile(Path + "extensions/" + args.Name.Substring(0, pos) + ".dll");
            }
            catch (Exception e)
            {
                _Log.WriteLine("Could not locate assembly '" + args.Name + "'");
                throw new Exception("Could not locate assembly '" + args.Name + "'", e);
            }
        }

        /// <summary>
        /// If the user closes the console via STRG + C, this function will be called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Dump();
        }

        internal void LogRequest(HttpRequest request, HttpResponse response)
        {
            lock (_Log)
            {
                _Log.WriteTimestamp();
                _Log.WriteColored(response.ClientHandler.IP.PadRight(14, ' '), ConsoleColor.Yellow);
                string status = Mapping.GetStatusCode(response.Header.Type);
                if (status.StartsWith("4") || status.StartsWith("5"))
                {
                    _Log.WriteColored(" " + Mapping.GetStatusCode(response.Header.Type), ConsoleColor.Red);
                }
                else
                {
                    _Log.WriteColored(" " + Mapping.GetStatusCode(response.Header.Type), ConsoleColor.Green);
                }
                _Log.WriteColored(" " + HttpRequest.GetRequestTypeName(request.Type), ConsoleColor.White);
                _Log.WriteColored(" " + request.File, ConsoleColor.Gray);
                _Log.WriteRightAlign(response.ContentLenght.ToString().PadLeft(5, ' '), ConsoleColor.DarkMagenta);
            }
        }
        
        #endregion

        #region State control

        /// <summary>
        /// Pause the server.
        /// </summary>
        public void Pause()
        {
            _Log.WriteLine("Server paused.");
            _Paused = true;
        }

        /// <summary>
        /// Exit the servers main loop.
        /// </summary>
        public void Shutdown()
        {
            _Log.WriteLine("Server shutdown.");
            _Exit = true;
            // destroy socket
            try
            {
                _Socket.Close();
            }
            catch { }
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The version of the server software.
        /// </summary>
        public string Version
        {
            get
            {
                return _Version;
            }
        }

        /// <summary>
        /// The log file handler of the server.
        /// </summary>
        public ILog Log
        {
            get { return _Log; }
        }

        /// <summary>
        /// All available projects running on this server.
        /// </summary>
        public IProjectCollection Projects
        {
            get
            {
                return _Projects;
            }
        }

        /// <summary>
        /// The configuration of the server (changeable during runtime).
        /// </summary>
        public Configuration Configuration
        {
            get
            {
                return _Configuration;
            }
        }

        /// <summary>
        /// Retrieve the path the server is running in.
        /// </summary>
        public string Path
        {
            get
            {
                if (_Path != null) return _Path;
                FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
                return _Path = file.Directory.FullName.Replace("\\", "/") + "/";
            }
        }

        /// <summary>
        /// Specifies, whether the server is currently congested or not. It the server has to many
        /// connections, it will enable the burst mode, which closes all keep-alive connections.
        /// </summary>
        public bool Congested
        {
            get
            {
                return _Congested;
            }
        }

        /// <summary>
        /// The server helper generates some pages for the GenHTTP webserver. This allows the
        /// customization of error pages or the server index.
        /// </summary>
        public IServerHelper Helper
        {
            get
            {
                return _Helper;
            }
        }

        /// <summary>
        /// Retrieve all running server instances.
        /// </summary>
        internal static Dictionary<int, Server> Instances
        {
            get { return _Instances; }
        }

        /// <summary>
        /// Retrieve a template of the standard server page.
        /// </summary>
        public IServerPage NewPage()
        {
            return new ServerPage(_ServerTemplate)
            {
                ServerVersion = Version
            };
        }

        public IContentProvider DefaultNotFoundProvider => new DefaultNotFoundProvider(this);

        public IContentProvider DefaultNotLoggedInProvider => new DefaultNotLoggedInProvider(this);

        public IContentProvider DefaultNotEnoughRightsProvider => new DefaultNotEnoughRightsProvider(this);

        public IContentProvider DefaultWrongParametersProvider => new DefaultWrongParametersProvider(this);

        #endregion

    }

}
