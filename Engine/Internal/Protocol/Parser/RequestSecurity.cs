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

        var target = request.Target.Path.Parts;

        for (var i = 0; i < target.Count; i++)
        {
            if (target[i].Value == "." || target[i].Value == "..")
            {
                throw new ProtocolException("Segments '.' or '..' are now allowed in path");
            }
        }
    }

}
