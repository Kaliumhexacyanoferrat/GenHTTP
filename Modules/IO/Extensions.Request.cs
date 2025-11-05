using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class RequestExtensions
{

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

    public static string? HostWithoutPort(this IRequest request)
    {
        var host = request.Host;

        if (host is not null)
        {
            var pos = host.IndexOf(':');

            return pos > 0 ? host[..pos] : host;
        }

        return null;
    }

}
