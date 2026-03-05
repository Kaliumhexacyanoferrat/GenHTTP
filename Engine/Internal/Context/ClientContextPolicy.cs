using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Engine.Internal.Context;

public class ClientContextPolicy : PooledObjectPolicy<ClientContext>
{

    public override ClientContext Create() => new(new());

    public override bool Return(ClientContext obj)
    {
        obj.Reset();
        
        return true;
    }

}
