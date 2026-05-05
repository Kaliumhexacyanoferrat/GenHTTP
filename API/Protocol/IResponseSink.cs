using System.Buffers;

namespace GenHTTP.Api.Protocol;

public interface IResponseSink
{

    IBufferWriter<byte> Writer { get; }
    
    Stream Stream { get; }
    
}
