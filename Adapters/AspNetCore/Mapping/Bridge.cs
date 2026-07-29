using GenHTTP.Adapters.AspNetCore.Server;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Adapters.AspNetCore.Context;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Adapters.AspNetCore.Mapping;

internal static class Bridge
{
    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), 1024);

    public static async Task MapAsync(HttpContext context, IHandler handler, IServer? server = null, bool requireResponse = false)
    {
        var actualServer = server ?? new ImplicitServer(context, handler);

        var clientContext = ContextPool.Get();

        try
        {
            clientContext.Apply(actualServer, context.Features);

            var connectionFeature = context.Features.Get<IHttpConnectionFeature>();
            var tlsFeature = context.Features.Get<ITlsConnectionFeature>();
            var requestFeature = context.Features.GetRequiredFeature<IHttpRequestFeature>();

            var endPoint = ResolveEndPoint(actualServer, connectionFeature, requestFeature);

            var request = clientContext.Request;

            request.Apply(actualServer, endPoint, context.Features, connectionFeature?.RemoteIpAddress, tlsFeature?.ClientCertificate);

            var headRequest = request.Header.Method == RequestMethod.Head;

            var response = await handler.HandleAsync(request);

            if (response == null)
            {
                // A root server (requireResponse) never sees a null response - only a
                // raw, unwrapped handler passed into Map()/Run() can return one, which
                // just means it didn't match, so it becomes a 404 instead.
                if (requireResponse)
                {
                    throw new InvalidOperationException("The root request handler did not return a response");
                }

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await clientContext.ResponseWriter.HandleAsync(request, response, headRequest);
        }
        finally
        {
            ContextPool.Return(clientContext);
        }
    }

    private static IEndPoint ResolveEndPoint(IServer server, IHttpConnectionFeature? connection, IHttpRequestFeature requestFeature)
    {
        var port = connection?.LocalPort;
        var secure = string.Equals(requestFeature.Scheme, "https", StringComparison.OrdinalIgnoreCase);

        IEndPoint? portMatch = null;

        foreach (var candidate in server.EndPoints)
        {
            if (candidate.Port == port)
            {
                if (candidate.Secure == secure)
                {
                    return candidate;
                }

                portMatch ??= candidate;
            }
        }

        return portMatch ?? server.EndPoints[0];
    }

}
