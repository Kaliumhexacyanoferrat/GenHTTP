using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Files.Multi;

public sealed class FileAssetsBuilder(DirectoryInfo directory) : IHandlerBuilder<FileAssetsBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<ICompressionAlgorithm> _algorithms = [];

    private char _separator = '.';

    private TimeSpan _refreshInterval = AssetRefresh.DefaultInterval;

    /// <summary>
    /// Configures the handler to serve pre-compressed files that are placed next
    /// to the requested files. If you pass the brotli algorithm to this method,
    /// the handler will look for a "file.txt.br" if "file.txt" is requested.
    /// </summary>
    /// <param name="algorithms">The supported algorithms for pre-compression</param>
    public FileAssetsBuilder AllowPrecompressed(params ICompressionAlgorithm[] algorithms)
    {
        _algorithms.AddRange(algorithms);
        return this;
    }

    /// <summary>
    /// Configures the handler to serve pre-compressed files that are placed next
    /// to the requested files. If you pass the brotli algorithm and "-" as a separator
    /// to this method,  the handler will look for a "file.txt-br" if "file.txt" is requested.
    /// </summary>
    /// <param name="algorithms">The supported algorithms for pre-compression</param>
    /// <param name="separator">The separator to use to build the paths</param>
    public FileAssetsBuilder AllowPrecompressed(ICompressionAlgorithm[] algorithms, char separator)
    {
        _separator = separator;

        return AllowPrecompressed(algorithms);
    }

    /// <summary>
    /// How often the mounted directory may be re-scanned for changes. Defaults to 250 ms and
    /// only affects the implementation on Ioxide.
    /// </summary>
    public FileAssetsBuilder RefreshInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.Zero && interval != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), interval, "The refresh interval must not be negative.");
        }

        _refreshInterval = interval;
        return this;
    }

    public FileAssetsBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, new FileAssetsHandler(directory, _algorithms, _separator, _refreshInterval));

}
