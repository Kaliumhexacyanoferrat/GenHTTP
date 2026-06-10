using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public sealed class ImperativeWebsocketContent(IImperativeHandler handler, IRequest request, ConnectionSettings settings) : IResponseContent
{

    public ulong? Length => null;

    public ContentType? Type => null;

    public ReadOnlyMemory<byte>? Encoding => null;

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        await sink.Stream.FlushAsync();

        await using var connection = new WebsocketConnection(request, sink, settings);

        await handler.HandleAsync(connection);
    }

}
