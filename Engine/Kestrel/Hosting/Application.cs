using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Kestrel.Context;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Engine.Kestrel.Hosting;

internal class Application(KestrelServerBridge server) : IHttpApplication<ClientContext>
{
    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), BufferSize.Write);

    public ClientContext CreateContext(IFeatureCollection contextFeatures)
    {
        var context = ContextPool.Get();
        context.Apply(server, contextFeatures);
        return context;
    }

    public async Task ProcessRequestAsync(ClientContext context)
    {
        try
        {
            await context.RequestHandler.HandleAsync();
        }
        catch (Exception e)
        {
            await SendErrorAsync(context, e);
        }
    }

    public void DisposeContext(ClientContext context, Exception? exception)
    {
        ContextPool.Return(context);
    }

    private static async Task SendErrorAsync(ClientContext context, Exception e)
    {
        try
        {
            var responseFeature = context.Features.GetRequiredFeature<IHttpResponseFeature>();

            if (responseFeature.HasStarted)
            {
                // headers (or worse, body bytes) are already on the wire - nothing sane to do
                return;
            }

            context.Server.Logging.CreateLogger<Application>().LogWarning(e, "Failed to handle client request");

            var message = context.Server.Development ? e.ToString() : "Internal Server Error";
            var body = Encoding.UTF8.GetBytes(message);

            responseFeature.StatusCode = (int)ResponseStatus.InternalServerError;
            responseFeature.Headers.ContentType = "text/plain";
            responseFeature.Headers.ContentLength = body.Length;

            var bodyFeature = context.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

            await bodyFeature.Stream.WriteAsync(body);
        }
        catch
        {
            /* no recovery here */
        }
    }

}
