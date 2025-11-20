using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Adapters.AspNetCore.Types;

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
