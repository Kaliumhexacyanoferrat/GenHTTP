using System;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Practices;

namespace GenHTTP.Testing
{

    /// <summary>
    /// Hosts GenHTTP projects on a random port and provides convenience functionality
    /// to test the responses of the server.
    /// </summary>
    public class TestHost : IDisposable
    {
#if NET8_0
        private static volatile int _NextPort = 20000;
#else
        private static volatile int _NextPort = 30000;
#endif

        private static readonly HttpClient _DefaultClient = GetClient();

        #region Get-/Setters

        /// <summary>
        /// The port this host listens to.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The host managed by this testing host.
        /// </summary>
        public IServerHost Host { get; private set; }

        #endregion

        #region Lifecycle

        static TestHost()
        {
            HttpWebRequest.DefaultCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
        }

        /// <summary>
        /// Creates a test host that will use the given handler to provide content, 
        /// but has yet to be started.
        /// </summary>
        /// <param name="handlerBuilder">The handler to be tested</param>
        /// <param name="defaults">true, if the defaults (such as compression) should be added to this handler</param>
        /// <param name="development">true, if the server should be started in development mode</param>
        public TestHost(IHandlerBuilder handlerBuilder, bool defaults = true, bool development = true)
        {
            Port = NextPort();

            Host = Engine.Host.Create()
                              .Handler(handlerBuilder)
                              .Port((ushort)Port);

            if (defaults)
            {
                Host.Defaults();
            }

            if (development)
            {
                Host.Development();
            }
        }

        /// <summary>
        /// Creates a test host that will use the given handler to provide content
        /// and starts it immediately.
        /// </summary>
        /// <param name="handlerBuilder">The handler to be tested</param>
        /// <param name="defaults">true, if the defaults (such as compression) should be added to this handler</param>
        /// <param name="development">true, if the server should be started in development mode</param>
        public static TestHost Run(IHandlerBuilder handlerBuilder, bool defaults = true, bool development = true)
        {
            var runner = new TestHost(handlerBuilder, defaults, development);

            runner.Start();

            return runner;
        }

        /// <summary>
        /// Starts the server managed by this testing host.
        /// </summary>
        /// <remarks>
        /// Dispose this runner to shut down the server and release all resources
        /// or close the host via the <see cref="Host" /> property.
        /// </remarks>
        public void Start()
        {
            Host.Start();
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Returns the next free port to be used by the testing host
        /// to provide content.
        /// </summary>
        /// <remarks>
        /// You typically do not need to call this method by yourself, as the
        /// test host runner methods will automatically claim the next free port.
        /// </remarks>
        /// <returns>The next free port to be used</returns>
        public static int NextPort() => Interlocked.Increment(ref _NextPort);

        /// <summary>
        /// Computes the URL which can be used to fetch the given path
        /// from the hosted server.
        /// </summary>
        /// <param name="path">The path to fetch from the server</param>
        /// <returns>The URL that can be used to fetch the content</returns>
        public string GetUrl(string? path = null) => $"http://localhost:{Port}{path ?? ""}";

        /// <summary>
        /// Fetches a request instance for the given path and method which
        /// can then be configured before being passed to the <see cref="GetResponseAsync(HttpRequestMessage, HttpClient?)" />
        /// method.
        /// </summary>
        /// <param name="path">The path the request should fetch</param>
        /// <param name="method">The method to be used for the request, if not GET</param>
        /// <returns>The newly created request message</returns>
        public HttpRequestMessage GetRequest(string? path = null, HttpMethod? method = null)
        {
            return new HttpRequestMessage(method ?? HttpMethod.Get, GetUrl(path));
        }

        /// <summary>
        /// Runs a GET request against the given path.
        /// </summary>
        /// <param name="path">The path to be fetched</param>
        /// <param name="client">The configured HTTP client to be used or null, if the default client should be used</param>
        /// <returns>The response returned by the server</returns>
        public async Task<HttpResponseMessage> GetResponseAsync(string? path = null, HttpClient? client = null)
        {
            var actualClient = client ?? _DefaultClient;

            return await actualClient.GetAsync(GetUrl(path));
        }

        /// <summary>
        /// Executes the given request against the test server.
        /// </summary>
        /// <param name="message">The request to be executed</param>
        /// <param name="client">The configured HTTP client to be used or null, if the default client should be used</param>
        /// <returns>The response returned by the server</returns>
        public async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage message, HttpClient? client = null)
        {
            var actualClient = client ?? _DefaultClient;

            return await actualClient.SendAsync(message);
        }

        public static HttpClient GetClient(bool ignoreSecurityErrors = false, bool followRedirects = false,
            Version? protocolVersion = null, NetworkCredential? creds = null, CookieContainer? cookies = null)
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
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true;
            }

            var client = new HttpClient(handler)
            {
                DefaultRequestVersion = protocolVersion ?? HttpVersion.Version11
            };

            client.DefaultRequestHeaders.ConnectionClose = false;

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

        /// <summary>
        /// Stops the test host and releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }


}
