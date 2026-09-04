using System.Net;
using System.Security.Authentication;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>Kernel TLS: the record layer moves into the kernel, in both directions.</summary>
public static class KernelTlsSample
{

    public static IServerHost Create()
    {
        /*
         *
         * kTLS hands record encryption to the kernel after the handshake. It needs the Linux
         * tls module loaded, or every handshake on the port fails:
         *
         *     cat /proc/sys/net/ipv4/tcp_available_ulp   # wanted: tls
         *     sudo modprobe tls
         *
         * Turning it on constrains the port. The kernel derives its keys from one ciphersuite
         * over TLS 1.3, so the floor is pinned to 1.3 and asking for 1.2 alongside is refused
         * when the server starts rather than silently losing. Session resumption is off, since
         * a ticket would consume a record sequence number and desynchronise the handoff.
         *
         * RxKernelTls needs TxKernelTls: inbound is programmed at the same handoff and shares
         * the TCP_ULP that the outbound side installs, so asking for RX alone is refused. With
         * both on, plaintext lands directly in ring memory and the zero-copy reader works on a
         * TLS connection exactly as it does on a cleartext one.
         *
         * It is not a free win, and it is off by default for that reason: on loopback it trails
         * OpenSSL on large writes and costs more CPU per request. Turn it on for what it is for
         * - sendfile, and NICs that offload TLS - and measure your own traffic.
         *
         *     curl -k --http1.1 https://localhost:8443/ok
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        var (certificate, key) = Certs.Server("localhost");

        return Host.Create(options: new EngineOptions
                   {
                       ProtocolsByPort = { [8443] = HttpProtocols.Http1AndHttp2 },

                       Tcp = new TcpTransportOptions
                       {
                           TxKernelTls = true,
                           RxKernelTls = true,
                       },
                   })
                   .Handler(app)
                   // Tls13 alone: kTLS pins it, and naming 1.2 here would be a contradiction.
                   .Bind(IPAddress.Loopback, 8443, new FileCertificateProvider(certificate, key),
                         httpProtocols: SslProtocols.Tls13);
    }

}
