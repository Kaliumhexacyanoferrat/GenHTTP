using System.Net;

using GenHTTP.Api.Infrastructure;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Server;

/// <summary>
/// Describes the endpoint a request was received on when GenHTTP is embedded into an
/// existing ASP.NET Core application - there is no GenHTTP-managed <see cref="IEndPoint"/>
/// in this mode, so this is derived from the current <see cref="HttpContext"/> instead.
/// </summary>
internal sealed class ImplicitEndpoint(HttpContext context) : IEndPoint
{

    public IPAddress? Address => context.Connection.LocalIpAddress;

    public ushort Port => (ushort)context.Connection.LocalPort;

    public bool DualStack => throw new NotSupportedException("Cannot determine whether dual stack is enabled in adapter mode");

    public bool Secure => context.Request.IsHttps;

    public void Dispose()
    {
        // nop - the connection is owned by ASP.NET Core, not by this endpoint
    }

}
