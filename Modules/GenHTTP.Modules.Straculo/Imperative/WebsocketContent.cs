using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Straculo.Utils;

namespace GenHTTP.Modules.Straculo.Imperative;

public abstract class WebsocketContent : IResponseContent
{
    public ulong? Length { get; } = null!;

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)GetHashCode());

    protected abstract ValueTask HandleAsync(WebsocketStream stream);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await HandleAsync(new WebsocketStream(target));
    }
}