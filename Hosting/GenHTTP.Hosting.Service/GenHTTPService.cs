using System;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using GenHTTP.Core;

namespace GenHTTP
{

    /// <summary>
    /// Runs the GenHTTP webserver as a Windows service.
    /// </summary>
    public class GenHTTPService : ServiceBase
    {
        private Server _Server;
        private Thread _Thread;

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        public GenHTTPService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method will be called when the service is started.
        /// </summary>
        /// <param name="args">The args to handle</param>
        protected override void OnStart(string[] args)
        {
            _Server = new Server();
            _Thread = new Thread(new ThreadStart(_Server.Listen));
            _Thread.Start();
        }

        /// <summary>
        /// This method will be called when the service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            _Server.Dump();
            _Server.Shutdown();
        }

        private void InitializeComponent()
        {
            this.ServiceName = "GenHTTP";
            this.CanStop = true;
        }

    }

}