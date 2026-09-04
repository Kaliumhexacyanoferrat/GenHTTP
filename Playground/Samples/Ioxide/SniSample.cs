using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>One port, a certificate per host name, chosen by the name the client asked for.</summary>
public static class SniSample
{

    public static IServerHost Create()
    {
        /*
         *
         * Server Name Indication (RFC 6066) is what lets one address serve several hosts over
         * TLS: the client sends the name it wants during the handshake, and the server answers
         * with that name's certificate.
         *
         * HostCertificateProvider holds one certificate per name plus a default. The default is
         * not optional - it answers a client that sent no name at all, or asked for one this
         * port does not hold, which is what a bare IP address does, since an IP is not a legal
         * SNI value. An unknown name is served the default rather than refused: aborting the
         * handshake would leave the client with a connection error instead of a certificate it
         * can reason about.
         *
         * Names are matched case-insensitively and exactly. A wildcard certificate covers its
         * names through the certificate itself, not by being registered here.
         *
         * Every name is named as files, which is what lets HTTP/3 serve them too - both stacks
         * settle their SNI tables when the server starts.
         *
         *     curl -kv --resolve alpha.localhost:8443:127.0.0.1 https://alpha.localhost:8443/ok
         *     curl -kv --resolve beta.localhost:8443:127.0.0.1  https://beta.localhost:8443/ok
         *     curl -kv https://localhost:8443/ok                # falls back to the default
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        var (defaultCertificate, defaultKey) = Certs.Server("localhost");

        var certificates = new HostCertificateProvider(defaultCertificate, defaultKey);

        foreach (var name in new[] { "alpha.localhost", "beta.localhost" })
        {
            var (certificate, key) = Certs.Server(name);

            certificates.Add(name, certificate, key);
        }

        return Host.Create(options: new EngineOptions { ProtocolsByPort = { [8443] = HttpProtocols.Http1AndHttp2 } })
                   .Handler(app)
                   .Bind(IPAddress.Loopback, 8443, certificates);
    }

}
