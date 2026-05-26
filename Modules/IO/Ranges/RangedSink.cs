using System.Buffers;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Ranges;

public class RangedSink(RangedStream stream) : IResponseSink
{
    private StreamBufferWriter? _writer;

    public IBufferWriter<byte> Writer => _writer ??= new StreamBufferWriter(Stream);

    public Stream Stream => stream;
    
}
