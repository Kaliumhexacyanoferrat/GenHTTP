using Fleck;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Handler;

internal static class Extensions
{

    internal static WebSocketHttpRequest Map(this IRequest request)
    {
        string? body = null;

        if (request.Content != null)
        {
            using var reader = new StreamReader(request.Content);

            body = reader.ReadToEnd();
        }

        var mapped = new WebSocketHttpRequest()
        {
            Method = request.Method.RawMethod,
            Path = request.Target.Path.ToString(true),
            Scheme = request.EndPoint.Secure ? "wss" : "ws",
            Bytes = [],
            Body = body ?? string.Empty
        };

        foreach (var (k, v) in request.Headers)
        {
            mapped.Headers.Add(k, v);
        }

        return mapped;
    }

}
