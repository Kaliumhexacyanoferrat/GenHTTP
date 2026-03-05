using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Context;

public record ClientContext(Request Request)
{

    public void Reset() => Request.Reset();

}
