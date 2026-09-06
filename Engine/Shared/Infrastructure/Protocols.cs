using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Shared.Infrastructure;

public static class Protocols
{

    public static HttpProtocols Validate(EndPointConfiguration config)
    {
        var requested = config.Protocols;

        if (config.Security is null)
        {
            requested &= ~HttpProtocols.Http3;
            
            if (requested == HttpProtocols.None)
            {
                throw new NotSupportedException("H3 can only be served over secure endpoints");
            }
        }
        else if (requested == HttpProtocols.None)
        {
            throw new NotSupportedException("At least one protocol has to be specified");
        }

        return requested;
    }
    
}
