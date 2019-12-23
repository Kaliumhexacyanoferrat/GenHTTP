using System;
using System.Net;
using System.Net.Cache;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;

namespace GenHTTP.Testing.Acceptance.Domain
{

    public class TestRunner : IDisposable
    {
        private static object _SyncRoot = new object();
        private static ushort _NextPort = 20000;

        #region Get-/Setters

        public ushort Port { get; }

        public IServerHost Host { get; protected set; }

        #endregion

        #region Initialization

        static TestRunner()
        {
            HttpWebRequest.DefaultCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
        }

        public TestRunner()
        {
            Port = NextPort();

            Host = GenHTTP.Core.Host.Create()
                                    .Router(Layout.Create())
                                    .Port(Port);
        }

        public static ushort NextPort()
        {
            lock (_SyncRoot)
            {
                return _NextPort++;
            }
        }

        public static TestRunner Run()
        {
            var runner = new TestRunner();

            runner.Host.Start();

            return runner;
        }

        public static TestRunner Run(IRouterBuilder router) => Run(router.Build());

        public static TestRunner Run(IRouter router)
        {
            var runner = new TestRunner();

            runner.Host.Router(router).Start();

            return runner;
        }

        #endregion

        #region Functionality

        public void Start()
        {
            Host.Start();
        }

        public HttpWebRequest GetRequest(string? uri = null)
        {
            var request = WebRequest.CreateHttp($"http://localhost:{Port}{uri ?? ""}");
            request.Timeout = 3000;
            request.AllowAutoRedirect = false;

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
                    Host.Stop();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
