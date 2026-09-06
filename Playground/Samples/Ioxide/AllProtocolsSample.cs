using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>One server serving all three protocols, which is what a browser actually meets.</summary>
public static class AllProtocolsSample
{

    public static IServerHost Create()
    {
        /*
         *
         * One host, a protocol combination per port:
         *
         *   http://localhost:8080     HTTP/1.1 in the clear
         *   http://localhost:8082     HTTP/1.1 and HTTP/2 on one socket (the preface decides)
         *   https://localhost:8443    HTTP/1.1 and HTTP/2 over TCP, and HTTP/3 over QUIC
         *
         * The secure port is the interesting one: HTTP/1.1 and HTTP/2 share a TCP socket where
         * ALPN chooses between them, and HTTP/3 is a UDP socket on the SAME port number. That
         * pairing is what makes the port reachable by a browser, which connects over TCP and
         * only moves to QUIC once an Alt-Svc header points it there - it never tries HTTP/3
         * first. Serving HTTP/3 alone, as Http3Sample does, is invisible to a browser.
         *
         * Each binding names its own protocols. HTTP/3 has no cleartext mode, so it is dropped
         * from any binding made without a certificate rather than failing the server; only the
         * secure 8443 below carries it, even though it asks for All.
         *
         *     curl http://localhost:8080/ok
         *     curl --http2-prior-knowledge http://localhost:8082/ok
         *     curl -k --http1.1    https://localhost:8443/ok
         *     curl -k --http2      https://localhost:8443/ok
         *     curl -k --http3-only https://localhost:8443/ok
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        var (certificate, key) = Certs.Server("localhost");

        return Host.Create(options: new EngineOptions
                   {
                       Reactor = new ReactorOptions { ReactorCount = 2 },
                   })
                   .Handler(app)
                   .Bind(IPAddress.Loopback, 8080, HttpProtocols.Http1)
                   .Bind(IPAddress.Loopback, 8082, HttpProtocols.Http1AndHttp2)
                   // Files, not an X509Certificate2: HTTP/3 on this port needs PEM by path.
                   .Bind(IPAddress.Loopback, 8443, new FileCertificateProvider(certificate, key), httpProtocols: HttpProtocols.All);
    }

}
