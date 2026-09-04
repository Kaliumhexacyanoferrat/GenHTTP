using System.Net;
using System.Security.Authentication;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>TLS on the TCP transports, with ALPN choosing between HTTP/1.1 and HTTP/2.</summary>
public static class TlsSample
{

    public static IServerHost Create()
    {
        /*
         *
         * TLS is terminated in OpenSSL, and the handshake rides the same io_uring reads and
         * writes as everything else. Where a port serves both protocols, ALPN decides: the
         * server offers h2 first, so a client that speaks it gets HTTP/2 and everyone else
         * falls back to HTTP/1.1.
         *
         * The certificate can come as files or as an X509Certificate2 here - unlike HTTP/3,
         * OpenSSL takes the certificate as data. Files are still preferred: OpenSSL reads a
         * chain file whole, so intermediates come from the file rather than the machine store,
         * and the private key never enters managed memory.
         *
         * SslProtocols names the floor. OpenSSL takes a minimum version and has no maximum, so
         * Tls12 | Tls13 means "1.2 and up" and Tls13 alone means 1.3-only.
         *
         *     curl -k --http1.1 https://localhost:8443/ok
         *     curl -k --http2   https://localhost:8443/ok
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        var (certificate, key) = Certs.Server("localhost");

        return Host.Create(options: new EngineOptions
                   {
                       Tcp = new TcpTransportOptions
                       {
                           // A peer that connects and then says nothing is swept rather than held.
                           HandshakeTimeoutMs = 10_000,
                       },
                   })
                   .Handler(app)
                   .Bind(IPAddress.Loopback, 8443, new FileCertificateProvider(certificate, key),
                         sslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13,
                         httpProtocols: HttpProtocols.Http1AndHttp2);
    }

}
