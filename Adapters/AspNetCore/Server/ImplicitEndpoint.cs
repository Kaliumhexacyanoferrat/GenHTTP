using System.Net;

using GenHTTP.Api.Infrastructure;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Server;

internal sealed class ImplicitEndpoint(HttpContext context) : IEndPoint
{

    public IPAddress? Address => context.Connection.LocalIpAddress;

    public ushort Port => (ushort)context.Connection.LocalPort;

    // Not observable from the request-scoped HttpContext this adapter is built from - only
    // meaningful for the bind-time configuration engines construct their own endpoints from.
    public bool DualStack => false;

    public bool Secure => context.Request.IsHttps;

    public void Dispose() { }

}
