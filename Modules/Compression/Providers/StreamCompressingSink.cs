using System.Buffers;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.Providers;

internal sealed class StreamCompressingSink : IResponseSink, IDisposable, IAsyncDisposable
{
    private StreamBufferWriter? _writer;

    internal StreamCompressingSink(IResponseSink originalSink, Func<Stream, Stream> generator)
    {
        Stream = generator(originalSink.Stream);
    }

    public IBufferWriter<byte> Writer => _writer ??= new StreamBufferWriter(Stream);

    public Stream Stream { get; }

    public void Dispose() => Stream.Dispose();

    public async ValueTask DisposeAsync() => await Stream.DisposeAsync();
}
