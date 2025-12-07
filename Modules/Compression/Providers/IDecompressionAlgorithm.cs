namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// The implementation of an algorithm allowing to decompress incoming
/// request content that has been compressed by the client.
/// </summary>
public interface IDecompressionAlgorithm
{
    /// <summary>
    /// The name of the algorithm as specified by the client in the
    /// "Content-Encoding" HTTP header.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Returns a stream that wraps the request content and decompresses on read.
    /// </summary>
    /// <param name="content">The compressed request content stream</param>
    /// <returns>A stream that provides decompressed content</returns>
    Stream Decompress(Stream content);
}
