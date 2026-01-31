using GenHTTP.Engine.Shared.Types;
using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Engine.Rocket.Protocol;

internal class ClientContext
{
    internal Request Request { get; }

    internal ResponseBuilder ResponseBuilder { get; }

    internal Response Response { get; }

    internal ClientContext()
    {
        Response = new Response();

        ResponseBuilder = new ResponseBuilder(Response);

        Request = new Request(ResponseBuilder);
    }

    internal void Reset()
    {
        Request.Reset();
        Response.Reset();
    }
}

internal class ClientContextPolicy : PooledObjectPolicy<ClientContext>
{
    public override ClientContext Create() => new();

    public override bool Return(ClientContext obj)
    {
        obj.Reset();
        return true;
    }
}
