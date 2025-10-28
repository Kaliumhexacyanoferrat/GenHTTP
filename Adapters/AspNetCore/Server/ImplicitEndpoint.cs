using System.Net;

using GenHTTP.Api.Infrastructure;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Server;

public class ImplicitEndpoint(HttpContext context) : IEndPoint
{

    public IPAddress? Address => context.Connection.LocalIpAddress;

    public ushort Port => (ushort)context.Connection.LocalPort;

    public bool Secure => context.Request.IsHttps;
    
    public void Dispose()
    {
        // nop
    }
    
}
