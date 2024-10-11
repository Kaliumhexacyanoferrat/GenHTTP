using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Compression;

public static class CompressedContent
{

    #region Builder

    /// <summary>
    ///     Creates an empty builder that can be configured by adding
    ///     compression algorithms.
    /// </summary>
    /// <returns>The newly created builder</returns>
    public static CompressionConcernBuilder Empty() => new();

    /// <summary>
    ///     Creates a pre-configured builder which already supports
    ///     Zstandard, Brotli and Gzip compression.
    /// </summary>
    /// <returns>The newly created builder</returns>
    public static CompressionConcernBuilder Default() => new CompressionConcernBuilder().Add(new ZstdCompression())
                                                                                        .Add(new BrotliCompression())
                                                                                        .Add(new GzipAlgorithm());

    #endregion

    #region Extensions

    /// <summary>
    ///     Configures the host to compress responses if the client supports
    ///     this feature.
    /// </summary>
    /// <param name="host">The host to be configured</param>
    /// <param name="compression">The compression builder to use for compression</param>
    /// <returns>The configured server host for chaining</returns>
    public static IServerHost Compression(this IServerHost host, CompressionConcernBuilder compression)
    {
        host.Add(compression);
        return host;
    }

    /// <summary>
    ///     Configures the host to compress responses if the client supports
    ///     this feature.
    /// </summary>
    /// <param name="host">The host to be configured</param>
    /// <returns>The configured server host for chaining</returns>
    public static IServerHost Compression(this IServerHost host) => host.Compression(Default());

    #endregion

}
