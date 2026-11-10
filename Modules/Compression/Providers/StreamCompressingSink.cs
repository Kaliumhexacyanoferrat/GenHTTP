using System.Buffers;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.Providers;

internal sealed class StreamCompressingSink : IResponseSink, IAsyncDisposable
{
    private StreamBufferWriter? _writer;

    internal StreamCompressingSink(Stream compressingStream)
    {
        Stream = compressingStream;
    }

    public IBufferWriter<byte> Writer => _writer ??= new StreamBufferWriter(Stream);

    public Stream Stream { get; }

    public async ValueTask DisposeAsync()
    {
        _writer?.Dispose();
        await Stream.DisposeAsync();
    }
}
