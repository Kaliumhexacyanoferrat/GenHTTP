using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Context;

internal record ClientContext(ClientHandler Handler, Request Request)
{

    public void Reset() => Request.Reset();

}
