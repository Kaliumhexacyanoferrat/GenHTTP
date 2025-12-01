using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public sealed class ImperativeWebsocketContent(IImperativeHandler handler) : IResponseContent
{

    public ulong? Length => null;

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var connection = new WebsocketConnection(target);

        await handler.HandleAsync(connection);
    }
    
}
