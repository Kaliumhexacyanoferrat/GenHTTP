using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>HTTP/3 over QUIC, on a port that serves nothing else.</summary>
public static class Http3Sample
{

    public static IServerHost Create()
    {
        /*
         *
         * QUIC carries TLS 1.3 and has no cleartext mode, so an HTTP/3 port always needs a
         * certificate - and it needs it AS FILES. ngtcp2 loads PEM by path and takes nothing
         * else, so an in-memory X509Certificate2 is refused at startup rather than quietly
         * serving only TCP. FileCertificateProvider is what names the two paths.
         *
         * Naming Http3 alone for the port is taken literally: no TCP listener is bound at all,
         * and a browser will not find this server, since browsers connect over TCP first and
         * move to QUIC only once an Alt-Svc header points them at it. AllProtocolsSample shows
         * the arrangement a browser can actually reach.
         *
         * Only one endpoint may serve HTTP/3 - the engine binds a single QUIC listener, and a
         * second is refused when the server starts.
         *
         *     curl -k --http3-only https://localhost:8443/ok
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        var (certificate, key) = Certs.Server("localhost");

        return Host.Create(options: new EngineOptions
                   {
                       ProtocolsByPort = { [8443] = Protocols.Http3 },

                       Quic = new QuicTransportOptions
                       {
                           HandshakeTimeoutMs = 10_000,
                           IdleTimeoutMs = 60_000,
                       },
                   })
                   .Handler(app)
                   .Bind(IPAddress.Loopback, 8443, new FileCertificateProvider(certificate, key));
    }

}
