using GenHTTP.Adapters.AspNetCore.Server;
using GenHTTP.Adapters.AspNetCore.Types;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Protocol;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Adapters.AspNetCore.Mapping;

public static class Bridge
{
    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), 65536);

    public static async ValueTask MapAsync(HttpContext context, IHandler handler, IServer? server = null, IServerCompanion? companion = null, string? registeredPath = null)
    {
        var actualServer = server ?? new ImplicitServer(context, handler, companion);

        var clientContext = ContextPool.Get();

        try
        {
            var request = clientContext.Request;

            request.Configure(actualServer, context);

            if (registeredPath != null)
            {
                AdvanceTo(clientContext.Request, registeredPath);
            }

            var response = await handler.HandleAsync(request);

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
        finally
        {
            ContextPool.Return(clientContext);
        }
    }

    private static async ValueTask WriteAsync(IResponse response, HttpContext context)
    {
        var target = context.Response;

        target.StatusCode = response.Status.RawStatus;

        if (response.Connection == Connection.Upgrade)
        {
            target.Headers.Append("Connection", "Upgrade");
        }
        else if (response.Connection == Connection.Close)
        {
            target.Headers.Append("Connection", "Close");
        }

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
