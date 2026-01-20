namespace GenHTTP.Engine.Internal.Protocol.Parser;

internal static class RequestSecurity
{

    public static void Validate(Request request)
    {
        if (!request.Headers.ContainsKey("Host"))
        {
            throw new ProtocolException("Mandatory 'Host' header is missing from the request");
        }

        if (request.ContainsMultipleHeaders("Host"))
        {
            throw new ProtocolException("Multiple 'Host' headers specified");
        }
    }

}
