using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Kestrel.Context;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
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

    public Task ProcessRequestAsync(ClientContext context) => throw new NotImplementedException();

    public void DisposeContext(ClientContext context, Exception? exception)
    {
        ContextPool.Return(context);
    }

}
