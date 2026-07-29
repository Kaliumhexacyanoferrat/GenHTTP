using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Kestrel.Context;

using Microsoft.AspNetCore.Http.Features;

namespace GenHTTP.Engine.Kestrel.Integration;

/// <summary>
/// Drives a single request through the GenHTTP handler chain.
/// </summary>
/// <remarks>
/// Mirrors <c>Internal/Protocol/ClientHandler.HandleRequestAsync</c> and
/// <c>Ioxide/Protocol/ConnectionDriver.HandleRequestAsync</c>, minus wire parsing (Kestrel
/// already parsed the request by the time <c>Application.ProcessRequestAsync</c> is called)
/// and minus keep-alive bookkeeping (Kestrel owns connection framing/reuse itself).
/// </remarks>
internal sealed class RequestHandler(ClientContext context)
{

    #region Functionality

    internal async ValueTask HandleAsync()
    {
        var features = context.Features;

        var connectionFeature = features.Get<IHttpConnectionFeature>();
        var tlsFeature = features.Get<ITlsConnectionFeature>();
        var requestFeature = features.GetRequiredFeature<IHttpRequestFeature>();

        var endPoint = ResolveEndPoint(context.Server, connectionFeature, requestFeature);

        var request = context.Request;

        request.Apply(context.Server, endPoint, features, connectionFeature?.RemoteIpAddress, tlsFeature?.ClientCertificate);

        var response = await context.Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var headRequest = request.Header.Method == RequestMethod.Head;

        await context.ResponseWriter.HandleAsync(request, response, headRequest);
    }

    /// <summary>
    /// Maps the connection this request arrived on back to the <see cref="IEndPoint"/> it
    /// was bound through, matched by local port (and, as a tiebreaker, by scheme) - Kestrel
    /// does not hand us the originating <see cref="IEndPoint"/> directly.
    /// </summary>
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

    #endregion

}
