using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

public sealed class WebsocketProxy(string upstream) : IHandler
{
    private const int ConnectionTimeout = 100_000; // same as HttpClient
    
    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var upstreamConnection = new RawWebsocketConnection(upstream);

        await upstreamConnection.InitializeStream();

        using var upgradeCts = new CancellationTokenSource(ConnectionTimeout);

        try
        {
            await upstreamConnection.TryUpgrade(request, token: upgradeCts.Token);
        }
        catch (ProviderException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ProviderException(ResponseStatus.GatewayTimeout, "Connection to the upstream host timed out.");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to upgrade upstream.", e);
        }

        var websocketHandler = new WebsocketHandler(_ => new WebsocketTunnelContent(upstreamConnection));

        return await websocketHandler.HandleAsync(request);
    }

    #endregion
    
}
