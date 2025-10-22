using GenHTTP.Adapters.WiredIO.Server;
using GenHTTP.Adapters.WiredIO.Types;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Wired.IO.Http11Express.Context;

using WR = Wired.IO.Protocol.Response;

namespace GenHTTP.Adapters.WiredIO.Mapping;

public static class Bridge
{

    public static async Task MapAsync(Http11ExpressContext context, Func<Http11ExpressContext, Task> next, IHandler handler, IServerCompanion? companion = null, string? registeredPath = null)
    {
        if ((registeredPath != null) && !context.Request.Route.StartsWith(registeredPath))
        {
            await next(context);
            return;
        }

        // todo: can we cache this somewhere?
        var server = new ImplicitServer(handler, companion);

        try
        {
            using var request = new Request(server, context.Request);

            if (registeredPath != null)
            {
                AdvanceTo(request, registeredPath);
            }

            using var response = await handler.HandleAsync(request);

            if (response != null)
            {
                MapResponse(response, context);

                server.Companion?.OnRequestHandled(request, response);
            }
            else
            {
                await next(context);
            }
        }
        catch (Exception e)
        {
            // todo: cannot tell the IP of the client in wired
            server.Companion?.OnServerError(ServerErrorScope.ServerConnection, null, e);
            throw;
        }
    }

    private static void MapResponse(IResponse response, Http11ExpressContext context)
    {
        var target = context.Respond();

        target.Status((WR.ResponseStatus)response.Status.RawStatus);

        foreach (var header in response.Headers)
        {
            target.Header(header.Key, header.Value);
        }

        if (response.Modified != null)
        {
            target.Header("Last-Modified", response.Modified.Value.ToUniversalTime().ToString("r"));
        }

        if (response.Expires != null)
        {
            target.Header("Expires", response.Expires.Value.ToUniversalTime().ToString("r"));
        }

        if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                target.Header("Set-Cookie", $"{cookie.Key}={cookie.Value.Value}");
            }
        }

        if (response.Content != null)
        {
            target.Content(new MappedContent(response));

            target.Header("Content-Type", (response.ContentType?.Charset != null ? $"{response.ContentType?.RawType}; charset={response.ContentType?.Charset}" : response.ContentType?.RawType) ?? "application/octet-stream");

            if (response.ContentEncoding != null)
            {
                target.Header("Content-Encoding", response.ContentEncoding);
            }
        }
    }

    private static void AdvanceTo(Request request, string registeredPath)
    {
        var parts = registeredPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var _ in parts)
        {
            request.Target.Advance();
        }
    }

}
