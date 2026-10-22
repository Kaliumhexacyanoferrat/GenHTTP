using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Practices;

using Microsoft.Extensions.Logging;

using Version = System.Version;

namespace GenHTTP.Testing;

/// <summary>
/// Hosts GenHTTP projects on a random port and provides convenience functionality
/// to test the responses of the server.
/// </summary>
public class TestHost : IAsyncDisposable
{

#if NET10_0
    private static volatile int _nextPort = 20010;
#elif NET11_0
    private static volatile int _nextPort = 22010;
#else
    private static volatile int _nextPort = 24010;
#endif

    // Analysis branch: the engine every test host runs on (TEST_ENGINE overrides whatever the
    // test asked for) and the protocol the ioxide hosts bind and the clients speak (TEST_PROTOCOL).
    private static readonly ServerEngine? ForcedEngine = ResolveEngine();

    private static readonly Lazy<FileCertificateProvider> Certificate = new(CreateCertificate);

    /// <summary>
    /// The protocol ioxide test hosts are bound to and test clients speak, read from the
    /// TEST_PROTOCOL environment variable (H1, H2 or H3, defaults to H1).
    /// </summary>
    public static TestProtocol Protocol { get; } = ResolveProtocol();

    private static readonly HttpClient DefaultClient = GetClient();

    private readonly CollectingLogger _log = new();

    #region Get-/Setters

    /// <summary>
    /// The port this host listens to.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// The host managed by this testing host.
    /// </summary>
    public IServerHost Host { get; }

    /// <summary>
    /// The engine this host actually runs on.
    /// </summary>
    public ServerEngine ActualEngine { get; }

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
    /// <param name="handler">The handler to be tested</param>
    /// <param name="defaults">true, if the defaults (such as compression) should be added to this handler</param>
    /// <param name="development">true, if the server should be started in development mode</param>
    /// <param name="serverEngine">The server engine to use for hosting</param>
    public TestHost(IHandler handler, bool defaults = true, bool development = true, ServerEngine serverEngine = ServerEngine.Internal)
    {
        Port = NextPort();

        ActualEngine = ForcedEngine ?? serverEngine;

        Host = CreateHost(ActualEngine);

        Host.Handler(handler);
        Host.Port((ushort)Port);
        Host.Logging(new LoggerFactory([_log]));

        if (ActualEngine == ServerEngine.Ioxide)
        {
            BindProtocol();
        }

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
    /// <param name="handler">The handler to be tested</param>
    /// <param name="defaults">true, if the defaults (such as compression) should be added to this handler</param>
    /// <param name="development">true, if the server should be started in development mode</param>
    /// <param name="engine">The server engine to use for hosting</param>
    public static async Task<TestHost> RunAsync(IHandler handler, bool defaults = true, bool development = true, ServerEngine engine = ServerEngine.Internal)
    {
        var runner = new TestHost(handler, defaults, development, engine);

        await runner.StartAsync();

        return runner;
    }

    /// <summary>
    /// Creates a test host that will use the given handler to provide content
    /// and starts it immediately.
    /// </summary>
    /// <param name="handler">The handler to be tested</param>
    /// <param name="defaults">true, if the defaults (such as compression) should be added to this handler</param>
    /// <param name="development">true, if the server should be started in development mode</param>
    /// <param name="engine">The server engine to use for hosting</param>
    public static Task<TestHost> RunAsync(IHandlerBuilder handler, bool defaults = true, bool development = true, ServerEngine engine = ServerEngine.Internal) 
        => RunAsync(handler.Build(), defaults, development, engine);

    /// <summary>
    /// Starts the server managed by this testing host.
    /// </summary>
    /// <remarks>
    /// Dispose this runner to shut down the server and release all resources
    /// or close the host via the <see cref="Host" /> property.
    /// </remarks>
    public async Task StartAsync()
    {
        // picked up by the summary script to tell tests that exercised a server from the rest
        Console.WriteLine($"[test-host] engine={ActualEngine} protocol={(ActualEngine == ServerEngine.Ioxide ? Protocol : TestProtocol.H1)} port={Port}");

        await Host.StartAsync();
    }

    // H1 keeps the default plain endpoint, H2 is bound alone in clear text (prior knowledge),
    // H3 alone over QUIC - so a client that does not speak the protocol under test fails.
    private void BindProtocol()
    {
        switch (Protocol)
        {
            case TestProtocol.H2:
                Host.Bind(null, (ushort)Port, HttpProtocols.Http2);
                break;
            case TestProtocol.H3:
                Host.Bind(null, (ushort)Port, Certificate.Value, httpProtocols: HttpProtocols.Http3);
                break;
        }
    }

    private static ServerEngine? ResolveEngine()
    {
        var engine = Environment.GetEnvironmentVariable("TEST_ENGINE");

        return Enum.TryParse(engine, true, out ServerEngine found) ? found : null;
    }

    private static TestProtocol ResolveProtocol()
    {
        var protocol = Environment.GetEnvironmentVariable("TEST_PROTOCOL");

        if (string.IsNullOrEmpty(protocol))
        {
            return TestProtocol.H1;
        }

        return Enum.TryParse(protocol, true, out TestProtocol found) ? found : throw new InvalidOperationException($"Protocol '{protocol}' is not supported");
    }

    // A self-signed localhost certificate as PEM files, which is the only form HTTP/3 takes.
    private static FileCertificateProvider CreateCertificate()
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        names.AddIpAddress(IPAddress.Loopback);
        names.AddIpAddress(IPAddress.IPv6Loopback);
        request.CertificateExtensions.Add(names.Build());

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "genhttp-test-host", Guid.NewGuid().ToString("n"))).FullName;

        var certificatePath = Path.Combine(dir, "localhost.crt");
        var keyPath = Path.Combine(dir, "localhost.key");

        File.WriteAllText(certificatePath, certificate.ExportCertificatePem());
        File.WriteAllText(keyPath, key.ExportPkcs8PrivateKeyPem());

        return new FileCertificateProvider(certificatePath, keyPath);
    }

    private static IServerHost CreateHost(ServerEngine serverEngine)
    {
        if (serverEngine == ServerEngine.Internal)
        {
            return Engine.Internal.Host.Create();
        }
        
        if (serverEngine == ServerEngine.Kestrel)
        {
            return Engine.Kestrel.Host.Create();
        }
        
        if (serverEngine == ServerEngine.Ioxide)
        {
            return Engine.Ioxide.Host.Create();
        }

        throw new NotSupportedException();
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
    public static int NextPort() => Interlocked.Increment(ref _nextPort);

    /// <summary>
    /// Computes the URL which can be used to fetch the given path
    /// from the hosted server.
    /// </summary>
    /// <param name="path">The path to fetch from the server</param>
    /// <returns>The URL that can be used to fetch the content</returns>
    public string GetUrl(string? path = null) => $"{(ActualEngine == ServerEngine.Ioxide && Protocol == TestProtocol.H3 ? "https" : "http")}://localhost:{Port}{path ?? ""}";

    /// <summary>
    /// Fetches a request instance for the given path and method which
    /// can then be configured before being passed to the <see cref="GetResponseAsync(HttpRequestMessage, HttpClient?)" />
    /// method.
    /// </summary>
    /// <param name="path">The path the request should fetch</param>
    /// <param name="method">The method to be used for the request, if not GET</param>
    /// <returns>The newly created request message</returns>
    public HttpRequestMessage GetRequest(string? path = null, HttpMethod? method = null) => new(method ?? HttpMethod.Get, GetUrl(path));

    /// <summary>
    /// Runs a GET request against the given path.
    /// </summary>
    /// <param name="path">The path to be fetched</param>
    /// <param name="client">The configured HTTP client to be used or null, if the default client should be used</param>
    /// <returns>The response returned by the server</returns>
    public async Task<HttpResponseMessage> GetResponseAsync(string? path = null, HttpClient? client = null)
    {
        var actualClient = client ?? DefaultClient;

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
        var actualClient = client ?? DefaultClient;

        return await actualClient.SendAsync(message);
    }

    public static HttpClient GetClient(bool ignoreSecurityErrors = false, bool followRedirects = false,
        Version? protocolVersion = null, NetworkCredential? creds = null, CookieContainer? cookies = null)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = followRedirects,
            Credentials = creds,
            UseProxy = false,
            UseCookies = cookies != null
        };

        if (cookies != null)
        {
            handler.CookieContainer = cookies;
        }

        if (ignoreSecurityErrors || Protocol == TestProtocol.H3)
        {
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true;
        }

        // request messages carry their own version (1.1 unless set), so the protocol under
        // test is forced on every request rather than just on the client defaults
        HttpMessageHandler pipeline = (protocolVersion is null && Protocol != TestProtocol.H1)
            ? new ProtocolHandler(GetProtocolVersion()) { InnerHandler = handler }
            : handler;

        var client = new HttpClient(pipeline)
        {
            DefaultRequestVersion = protocolVersion ?? HttpVersion.Version11,
            Timeout = TimeSpan.FromSeconds(15)
        };

        client.DefaultRequestHeaders.ConnectionClose = false;

        return client;
    }

    /// <summary>
    /// The HTTP version the test clients should use to talk to the test hosts.
    /// </summary>
    public static Version GetProtocolVersion() => Protocol switch
    {
        TestProtocol.H2 => HttpVersion.Version20,
        TestProtocol.H3 => HttpVersion.Version30,
        _ => HttpVersion.Version11
    };

    #endregion

    #region IDisposable Support

    private bool _disposed;

    protected async virtual ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                await Host.StopAsync();

                foreach (var entry in _log.Entries)
                {
                    Console.WriteLine($"[test-host] {entry}");
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Stops the test host and releases all resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    #endregion

}

/// <summary>
/// Sends every request with exactly the protocol under test (analysis only).
/// </summary>
internal sealed class ProtocolHandler(Version version) : DelegatingHandler
{

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Version = version;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

        return base.SendAsync(request, cancellationToken);
    }

}

/// <summary>
/// The protocol test hosts are bound to (analysis only).
/// </summary>
public enum TestProtocol
{
    H1,
    H2,
    H3
}
