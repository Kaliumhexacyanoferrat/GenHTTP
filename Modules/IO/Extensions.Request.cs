using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class RequestExtensions
{

    public static bool HasType(this IRequest request, params RequestMethod[] methods)
    {
        foreach (var method in methods)
        {
            if (request.Header.Method == method)
            {
                return true;
            }
        }

        return false;
    }

    public static ByteString? GetHostWithoutPort(this IRequest request)
    {
        var host = request.Header.Headers.GetEntry(KnownHeaders.Host);

        if (host == null)
        {
            return null;
        }

        var hostSpan = host.Value.Bytes.Span;
        var colonIndex = hostSpan.IndexOf((byte)':');

        return colonIndex >= 0 ? new(host.Value.Bytes[..colonIndex]) : host.Value;
    }

}
