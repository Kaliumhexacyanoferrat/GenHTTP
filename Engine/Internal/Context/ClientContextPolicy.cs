using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Engine.Internal.Context;

internal sealed class ClientContextPolicy : PooledObjectPolicy<ClientContext>
{

    public override ClientContext Create() => new();

    public override bool Return(ClientContext obj)
    {
        obj.Reset();
        
        return true;
    }

}
