using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.ServerCaching.Provider;

public class StreamSink(Stream target) : IResponseSink
{
    private readonly StreamBufferWriter _writer = new(target);

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => target;

}
