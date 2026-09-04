using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples.Ioxide;

/// <summary>The least the engine can do: HTTP/1.1 in the clear, on one port.</summary>
public static class Http1Sample
{

    public static IServerHost Create()
    {
        /*
         *
         * HTTP/1.1 needs no certificate and no negotiation - it is what the engine serves when
         * nothing else was agreed. Protocols.Http1 is the default, so this binding would behave
         * the same with no EngineOptions at all; it is named here to make the sample explicit.
         *
         *     curl http://localhost:8080/ok
         *
         */

        var app = Layout.Create()
                        .Add("ok", Content.From(Resource.FromString("ok")));

        return Host.Create(options: new EngineOptions { HttpProtocols = HttpProtocols.Http1 })
                   .Handler(app)
                   .Bind(IPAddress.Loopback, 8080);
    }

}
