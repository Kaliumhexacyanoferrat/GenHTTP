using System.IO.Compression;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressionConcernBuilder : IConcernBuilder
{
    private readonly List<ICompressionAlgorithm> _Algorithms = [];

    private CompressionLevel _Level = CompressionLevel.Fastest;

    private ulong? _MinimumSize = 256;

    #region Functionality

    public CompressionConcernBuilder Add(IBuilder<ICompressionAlgorithm> algorithm) => Add(algorithm.Build());

    public CompressionConcernBuilder Add(ICompressionAlgorithm algorithm)
    {
        _Algorithms.Add(algorithm);
        return this;
    }

    public CompressionConcernBuilder Level(CompressionLevel level)
    {
        _Level = level;
        return this;
    }

    /// <summary>
    /// Sets the minimum size threshold for compression. Content smaller than this
    /// threshold will not be compressed to avoid unnecessary overhead.
    /// </summary>
    /// <param name="minimumSize">The minimum size in bytes. Set to null to disable the threshold.</param>
    /// <returns>The builder instance for method chaining</returns>
    public CompressionConcernBuilder MinimumSize(ulong? minimumSize)
    {
        _MinimumSize = minimumSize;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var algorithms = _Algorithms.ToDictionary(a => a.Name);

        return new CompressionConcern(content, algorithms, _Level, _MinimumSize);
    }

    #endregion

}
