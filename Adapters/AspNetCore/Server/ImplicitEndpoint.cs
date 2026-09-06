using System.Net;

using GenHTTP.Api.Infrastructure;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Server;

internal sealed class ImplicitEndpoint(HttpContext context) : IEndPoint
{

    public IPAddress? Address => context.Connection.LocalIpAddress;

    public ushort Port => (ushort)context.Connection.LocalPort;

    public HttpProtocols Protocols => HttpProtocols.None; // we cannot tell

    public bool DualStack => false; // we cannot tell

    public bool Secure => context.Request.IsHttps;

    public void Dispose() { }

}
