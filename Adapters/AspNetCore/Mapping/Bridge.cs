using GenHTTP.Adapters.AspNetCore.Server;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Adapters.AspNetCore.Context;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using Microsoft.Extensions.ObjectPool;

namespace GenHTTP.Adapters.AspNetCore.Mapping;

/// <summary>
/// Bridges an ASP.NET Core <see cref="HttpContext"/> to the GenHTTP handler chain.
/// </summary>
/// <remarks>
/// Built on this package's own <see cref="Context"/> types (they already operate on
/// <see cref="IFeatureCollection"/>, which <see cref="HttpContext.Features"/> is) - the same ones
/// <c>GenHTTP.Engine.Kestrel</c> uses for its own request handling. One thing intentionally does
/// *not* match the engine's own orchestration: a <c>null</c> response here means the caller handed
/// us a raw, unwrapped handler that simply didn't match - not an engine-level invariant violation -
/// so it becomes a 404 instead of an exception (mirrors the pre-removal adapter's behavior;
/// GenHTTP's own engines only see a top-level handler already wrapped by CoreRouter/ErrorHandler,
/// which never returns null, hence those throw instead).
/// </remarks>
internal static class Bridge
{
    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), 1024);

    public static async Task MapAsync(HttpContext context, IHandler handler, IServer? server = null)
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
