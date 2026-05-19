using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Modules.IO;

public static class RequestExtensions
{
    private static readonly ReadOnlyMemory<byte> HostHeader = "Host"u8.ToArray();

    public static bool HasType(this IRequest request, params RequestMethod[] methods)
    {
        foreach (var method in methods)
        {
            if (request.Method == method)
            {
                return true;
            }
        }

        return false;
    }

    public static ReadOnlyMemory<byte>? GetHostWithoutPort(this IRequest request)
    {
        var host = request.Raw.Header.Headers.GetEntry(HostHeader);

        if (host == null)
        {
            return null;
        }

        var hostSpan = host.Value.Span;
        var colonIndex = hostSpan.IndexOf((byte)':');

        return colonIndex >= 0 ? host.Value[..colonIndex] : host.Value;
    }

}
