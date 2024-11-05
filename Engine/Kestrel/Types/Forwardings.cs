using GenHTTP.Api.Protocol;

using Microsoft.AspNetCore.Http;

namespace GenHTTP.Engine.Kestrel.Types;

public class Forwardings : List<Forwarding>, IForwardingCollection
{

    public Forwardings(HttpRequest request)
    {

    }

}
