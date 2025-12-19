using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public sealed class ImperativeWebsocketContent(IImperativeHandler handler, IRequest request, ConnectionSettings settings) : IResponseContent
{

    public ulong? Length => null;

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.FlushAsync();

        await using var connection = new WebsocketConnection(request, target, settings);

        await handler.HandleAsync(connection);
    }

}
