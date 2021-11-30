using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance
{

    public class TestRunner : IDisposable
    {
        private static volatile int _NextPort = 20000;

        private static HttpClient _DefaultClient = GetClient();

        #region Get-/Setters

        public int Port { get; }

        public IServerHost Host { get; private set; }

        #endregion

        #region Initialization

        static TestRunner()
        {
            HttpWebRequest.DefaultCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
        }

        public TestRunner(bool defaults = false, bool rangeSupport = false)
        {
            Port = NextPort();

            Host = GenHTTP.Engine.Host.Create()
                                      .Handler(Layout.Create())
                                      .Port((ushort)Port);

            if (defaults)
            {
                Host.Defaults(rangeSupport: rangeSupport);
            }
        }

        public static int NextPort() => Interlocked.Increment(ref _NextPort);

        public static TestRunner Run(bool defaults = true, bool rangeSupport = false)
        {
            var runner = new TestRunner(defaults, rangeSupport);

            runner.Host.Start();

            return runner;
        }

        public static TestRunner Run(IHandler handler, bool defaults = true, bool rangeSupport = false) => Run(handler.Wrap(), defaults, rangeSupport);

        public static TestRunner Run(IHandlerBuilder handlerBuilder, bool defaults = true, bool rangeSupport = false)
        {
            var runner = new TestRunner(defaults, rangeSupport);

            runner.Host.Handler(handlerBuilder).Start();

            return runner;
        }

        #endregion

        #region Functionality

        public void Start()
        {
            Host.Start();
        }


        public string GetUrl(string? path = null) => $"http://localhost:{Port}{path ?? ""}";

        public HttpRequestMessage GetRequest(string? path = null, HttpMethod? method = null)
        {
            return new HttpRequestMessage(method ?? HttpMethod.Get, GetUrl(path));
        }

        public async Task<HttpResponseMessage> GetResponse(string? path = null, HttpClient? client = null)
        {
            var actualClient = client ?? _DefaultClient;

            return await actualClient.GetAsync(GetUrl(path));
        }

        public async Task<HttpResponseMessage> GetResponse(HttpRequestMessage message, HttpClient? client = null)
        {
            var actualClient = client ?? _DefaultClient;

            return await actualClient.SendAsync(message);
        }

        public static HttpClient GetClient(bool ignoreSecurityErrors = false, bool followRedirects = true,
            Version? version = null, NetworkCredential? creds = null, CookieContainer? cookies = null)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = followRedirects,
                Credentials = creds
            };

            if (cookies != null)
            {
                handler.CookieContainer = cookies;
            }

            if (ignoreSecurityErrors)
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;
            }

            var client = new HttpClient(handler)
            {
                DefaultRequestVersion = version ?? HttpVersion.Version11,
            };

            return client;
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
