using System.Net;
using System.Runtime.InteropServices;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using IoxideServer = GenHTTP.Engine.Ioxide.Infrastructure.Server;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>Replacing the certificates of a running server, without dropping a connection.</summary>
public static class CertificateRotationSample
{

    // The registration has to outlive Create, or it is collected and the signal stops arriving.
    private static PosixSignalRegistration? _renewal;

    public static IServerHost Create()
    {
        /*
         *
         * A renewed certificate used to mean a restart, which drops every connection on the
         * server at the time. ReloadCertificates asks each bound provider again and installs
         * what it answers with, across both transports, while the server keeps serving.
         *
         * This is the shape an ACME hook takes: rewrite the PEM the providers already name,
         * then ask the server to install it. Connections already established keep the
         * certificate they authenticated with, since that is what their peer verified.
         *
         * Only the certificate material changes. Trust anchors, RequireCertificate, ALPN and
         * the TLS floor stay as the binding set them, and no NAME can be added - both stacks
         * settle their SNI tables at startup, so a reload replaces the certificates behind the
         * names rather than the set of them.
         *
         * Everything is resolved and checked before anything is published, so a provider that
         * throws, or a path an ACME client has not finished writing, leaves the server exactly
         * as it was. A half-rotated server is worse than one still serving yesterday's
         * certificate.
         *
         *     kill -HUP <pid>
         *     curl -k https://localhost:8443/ok      # same connection, new serial afterwards
         *     openssl s_client -connect localhost:8443 </dev/null | openssl x509 -noout -serial
         *
         */

        string[] names = ["localhost", "alpha.localhost"];

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        var (defaultCertificate, defaultKey) = Certs.Server(names[0]);

        var certificates = new HostCertificateProvider(defaultCertificate, defaultKey);

        var (alpha, alphaKey) = Certs.Server(names[1]);

        certificates.Add(names[1], alpha, alphaKey);

        var host = Host.Create(options: new EngineOptions { ProtocolsByPort = { [8443] = Protocols.Http1AndHttp2 } })
                       .Handler(app)
                       .Bind(IPAddress.Loopback, 8443, certificates);

        _renewal = PosixSignalRegistration.Create(PosixSignal.SIGHUP, context =>
        {
            context.Cancel = true;

            foreach (var name in names)
            {
                Certs.Server(name);
            }

            // The providers already name these files, so installing them is all that is left.
            (host.Instance as IoxideServer)?.ReloadCertificates();
        });

        Console.WriteLine($"kill -HUP {Environment.ProcessId} rotates the certificates");

        return host;
    }

}
