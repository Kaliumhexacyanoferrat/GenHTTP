using GenHTTP.Adapters.WiredIO.Server;
using GenHTTP.Adapters.WiredIO.Types;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Wired.IO.Http11.Context;

namespace GenHTTP.Adapters.WiredIO.Mapping;

public static class Bridge
{

    public static async ValueTask MapAsync(Http11Context context, IHandler handler, IServer? server = null, IServerCompanion? companion = null, string? registeredPath = null)
    {
        var actualServer = server ?? new ImplicitServer(handler, companion);

        try
        {
            using var request = new Request(actualServer, context);

            if (registeredPath != null)
            {
                AdvanceTo(request, registeredPath);
            }

            using var response = await handler.HandleAsync(request);

            if (response == null)
            {
                context.Response.StatusCode = 404;
            }
            else
            {
                await WriteAsync(response, context);

                actualServer.Companion?.OnRequestHandled(request, response);
            }
        }
        catch (Exception e)
        {
            actualServer.Companion?.OnServerError(ServerErrorScope.ServerConnection, context.Connection.RemoteIpAddress, e);
            throw;
        }
    }

    private static async ValueTask WriteAsync(IResponse response, HttpContext context)
    {
        var target = context.Response;

        target.StatusCode = response.Status.RawStatus;

        foreach (var header in response.Headers)
        {
            target.Headers.Append(header.Key, header.Value);
        }

        if (response.Modified != null)
        {
            target.Headers.LastModified = response.Modified.Value.ToUniversalTime().ToString("r");
        }

        if (response.Expires != null)
        {
            target.Headers.Expires = response.Expires.Value.ToUniversalTime().ToString("r");
        }

        if (response.HasCookies)
        {
            foreach (var cookie in response.Cookies)
            {
                if (cookie.Value.MaxAge != null)
                {
                    target.Cookies.Append(cookie.Key, cookie.Value.Value, new()
                    {
                        MaxAge = TimeSpan.FromSeconds(cookie.Value.MaxAge.Value)
                    });
                }
                else
                {
                    target.Cookies.Append(cookie.Key, cookie.Value.Value);
                }
            }
        }

        if (response.Content != null)
        {
            target.ContentLength = (long?)response.ContentLength ?? (long?)response.Content.Length;

            target.ContentType = response.ContentType?.Charset != null ? $"{response.ContentType?.RawType}; charset={response.ContentType?.Charset}" : response.ContentType?.RawType;

            if (response.ContentEncoding != null)
            {
                target.Headers.ContentEncoding = response.ContentEncoding;
            }

            await response.Content.WriteAsync(target.Body, 65 * 1024);
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
