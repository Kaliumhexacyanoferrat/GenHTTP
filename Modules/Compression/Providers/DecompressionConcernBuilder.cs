using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Builder for configuring automatic request content decompression.
/// </summary>
public sealed class DecompressionConcernBuilder : IConcernBuilder
{
    private readonly List<ICompressionAlgorithm> _algorithms = [];

    #region Functionality

    /// <summary>
    /// Adds a decompression algorithm built from a builder.
    /// </summary>
    /// <param name="algorithm">The algorithm builder to add</param>
    /// <returns>The builder instance for method chaining</returns>
    public DecompressionConcernBuilder Add(IBuilder<ICompressionAlgorithm> algorithm) => Add(algorithm.Build());

    /// <summary>
    /// Adds a decompression algorithm.
    /// </summary>
    /// <param name="algorithm">The algorithm to add</param>
    /// <returns>The builder instance for method chaining</returns>
    public DecompressionConcernBuilder Add(ICompressionAlgorithm algorithm)
    {
        _algorithms.Add(algorithm);
        return this;
    }

    /// <summary>
    /// Builds the decompression concern with the configured algorithms.
    /// </summary>
    /// <param name="content">The handler to wrap</param>
    /// <returns>The configured decompression concern</returns>
    public IConcern Build(IHandler content)
    {
        var algorithms = _algorithms.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

        return new DecompressionConcern(content, algorithms);
    }

    #endregion

}
