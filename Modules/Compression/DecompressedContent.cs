using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression;

/// <summary>
/// Provides automatic decompression of incoming request content.
/// </summary>
public static class DecompressedContent
{
    #region Builder

    /// <summary>
    /// Creates an empty builder that can be configured by adding
    /// decompression algorithms.
    /// </summary>
    /// <returns>The newly created builder</returns>
    public static DecompressionConcernBuilder Empty() => new();

    /// <summary>
    /// Creates a pre-configured builder which already supports
    /// Zstandard, Brotli, Gzip, and Deflate decompression.
    /// </summary>
    /// <returns>The newly created builder</returns>
    public static DecompressionConcernBuilder Default() => new DecompressionConcernBuilder()
        .Add(new ZstdDecompression())
        .Add(new BrotliDecompression())
        .Add(new GzipDecompression())
        .Add(new DeflateDecompression());

    #endregion

    #region Extensions

    /// <summary>
    /// Configures the host to automatically decompress incoming request content
    /// if it has been compressed by the client.
    /// </summary>
    /// <param name="host">The host to be configured</param>
    /// <param name="decompression">The decompression builder to use</param>
    /// <returns>The configured server host for chaining</returns>
    public static IServerHost Decompression(this IServerHost host, DecompressionConcernBuilder decompression)
    {
        host.Add(decompression);
        return host;
    }

    /// <summary>
    /// Configures the host to automatically decompress incoming request content
    /// if it has been compressed by the client.
    /// </summary>
    /// <param name="host">The host to be configured</param>
    /// <returns>The configured server host for chaining</returns>
    public static IServerHost Decompression(this IServerHost host) => host.Decompression(Default());

    #endregion
}
