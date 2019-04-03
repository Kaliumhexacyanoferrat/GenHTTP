using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;

using GenHTTP.Core;
using GenHTTP.Modules.Core;

namespace GenHTTP.Testing.Acceptance.Domain
{

    public class TestRunner : IDisposable
    {
        private static object _SyncRoot = new object();
        private static ushort _NextPort = 1024;

        #region Get-/Setters

        public ushort Port { get; }

        public IServerBuilder Builder { get; }
        
        public IServer? Instance { get; protected set; }

        #endregion

        #region Initialization
        
        static TestRunner()
        {
            HttpWebRequest.DefaultCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
        }

        public TestRunner()
        {
            lock (_SyncRoot)
            {
                Port = _NextPort++;

                Builder = Server.Create()
                                .Router(Layout.Create())
                                .Port(Port);
            }
        }

        public static TestRunner Run()
        {
            var runner = new TestRunner();

            runner.Instance = runner.Builder.Build();

            return runner;
        }

        public static TestRunner Run(IRouterBuilder router)
        {
            var runner = new TestRunner();

            runner.Instance = runner.Builder.Router(router).Build();

            return runner;
        }

        #endregion

        #region Functionality

        public HttpWebRequest GetRequest(string? uri = null)
        {
            var request = WebRequest.CreateHttp($"http://localhost:{Port}{uri ?? ""}");
            request.Timeout = 2000;

            return request;
        }

        public HttpWebResponse GetResponse(string? uri = null)
        {
            return GetRequest(uri).GetSafeResponse();
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Instance?.Dispose();
                    Instance = null;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

}
