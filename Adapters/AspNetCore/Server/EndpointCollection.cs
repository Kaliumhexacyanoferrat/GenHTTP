using GenHTTP.Api.Infrastructure;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Server;

internal sealed class EndpointCollection : List<IEndPoint>, IEndPointCollection
{

    public EndpointCollection(HttpContext context)
    {
        Add(new ImplicitEndpoint(context));
    }

}
