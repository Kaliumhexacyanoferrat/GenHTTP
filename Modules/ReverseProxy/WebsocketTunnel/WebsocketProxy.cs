using System.Security.Cryptography;
using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Handler;

namespace GenHTTP.Modules.ReverseProxy.WebsocketTunnel;

public class WebsocketProxy : IHandler
{
    private readonly string _upstreamUrl;
    
    public WebsocketProxy(string upstreamUrl)
    {
        _upstreamUrl = upstreamUrl;
    }
    
    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // This logic is redundant here, should be moved to this method's caller
        if (!request.Headers.ContainsKey("Upgrade") || request.Headers["Upgrade"] != "websocket")
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Websocket upgrade request expected");
        }

        //var upgradeInfo = request.Upgrade();
        //upgradeInfo.Socket.NoDelay = true;
        
        // Frozen request
        var clone = ClonedRequest.From(request);
        
        var upstreamConnection = new RawWebsocketConnection(_upstreamUrl);
        await upstreamConnection.InitializeStream();
        
        // Establish a websocket with upstream - use same key
        var upgradeCts = new CancellationTokenSource(5000);

        // TODO: Replace route
        if (!await upstreamConnection.TryUpgrade(clone.Headers, route: "/", token: upgradeCts.Token))
        {
            throw new InvalidOperationException("Failed to upgrade upstream.");
        }
        
        // Websocket connection with upstream is Ok, return this to establish websocket connection with downstream too
        // Use IResponseContent?
        
        //return upgradeInfo.Response;
        
        var key = clone.Headers.GetValueOrDefault("Sec-WebSocket-Key")
                  ?? throw new InvalidOperationException("Sec-WebSocket-Key not found");
        
        var response = request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", CreateAcceptKey(key))
            .Content(new WebsocketTunnelContent(upstreamConnection.Pipe!))
            .Build();
        
        return response;
    }
    
    // Adding this helper temporarily because Websocket changes are not yet merged
    private static string CreateAcceptKey(string key)
    {
        const string magicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(key + magicString));
        return Convert.ToBase64String(hash);
    }

    // Pure delegates
    public static Func<Stream, Task> UpstreamHandler = async upstream =>
    {
        await Task.Delay(1);
    };

    public static Func<Stream, Task> DownstreamHandler = async stream =>
    {
        await Task.Delay(1);
    };
}