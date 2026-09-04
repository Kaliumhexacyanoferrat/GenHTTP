using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>HTTP/2 without TLS (h2c), on its own port and sharing one with HTTP/1.1.</summary>
public static class Http2Sample
{

    public static IServerHost Create()
    {
        /*
         *
         * Two ways to serve HTTP/2 in the clear:
         *
         *   8081  HTTP/2 only. A client must already know to speak it - there is no upgrade
         *         dance here - and an HTTP/1.1 client is turned away.
         *   8082  both protocols on one socket. The engine peeks for the HTTP/2 connection
         *         preface without consuming it, so whichever arrives is handled.
         *
         * Browsers never do this: they require TLS for HTTP/2 and pick it through ALPN, which
         * is what TlsSample shows. h2c is for a trusted network or a load balancer behind TLS.
         *
         *     curl --http2-prior-knowledge http://localhost:8081/ok
         *     curl --http2-prior-knowledge http://localhost:8082/ok
         *     curl --http1.1               http://localhost:8082/ok
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        return Host.Create(options: new EngineOptions
                   {
                       ProtocolsByPort =
                       {
                           [8081] = HttpProtocols.Http2,
                           [8082] = HttpProtocols.Http1AndHttp2,
                       },
                   })
                   .Handler(app)
                   .Bind(IPAddress.Loopback, 8081)
                   .Bind(IPAddress.Loopback, 8082);
    }

}
