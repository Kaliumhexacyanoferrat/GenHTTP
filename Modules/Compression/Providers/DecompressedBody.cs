using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class DecompressedBody(IRequestBody body, ICompressionAlgorithm algorithm) : IRequestBody, IDisposable, IAsyncDisposable
{
    private readonly Stream _decompressedStream = algorithm.Decompress(body.AsStream());

    public Stream AsStream() => _decompressedStream;

    public async ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
    {
        using var memory = new MemoryStream();

        await _decompressedStream.CopyToAsync(memory);

        return memory.ToArray();
    }

    public void Dispose()
    {
        _decompressedStream.Dispose();
    }

    public ValueTask DisposeAsync() => _decompressedStream.DisposeAsync();

}
