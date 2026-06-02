using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Compression.PreCompression;

public readonly struct SupportedCompression(ICompressionAlgorithm algorithm, ReadOnlyMemory<byte> extension)
{

    public ICompressionAlgorithm Algorithm => algorithm;
    
    public ReadOnlyMemory<byte> Extension => extension;
    
}
