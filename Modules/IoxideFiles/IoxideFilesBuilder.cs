using GenHTTP.Api.Content;

using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Builds an <see cref="IoxideFilesHandler"/> over a shared <see cref="StaticAssets"/> cache and lets
/// concerns be chained onto it. Created via <see cref="IoxideFiles.From(string)"/>.
/// </summary>
public sealed class IoxideFilesBuilder : IHandlerBuilder<IoxideFilesBuilder>
{
    private readonly string _directory;

    private readonly StaticAssets _assets;

    private readonly List<IConcernBuilder> _concerns = [];

    private readonly long _stamp;

    private TimeSpan _refreshInterval = AssetRefresh.DefaultInterval;

    internal IoxideFilesBuilder(string directory)
    {
        _directory = directory;

        // Opened once and shared across reactors (fds are stable, reads positional); the per-reactor
        // AssetReader pool is resolved lazily in the content.
        _assets = new StaticAssets(directory);

        // Stamped here, with the snapshot, so the two describe the same moment - see the parameter
        // note on the AssetRefresh constructor for what taking it later would cost.
        _stamp = AssetRefresh.Stamp(directory);
    }

    public IoxideFilesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    /// <summary>
    /// How often the mounted directory may be re-scanned for changes. Defaults to 250 ms.
    /// </summary>
    /// <param name="interval">
    /// The shortest time between two scans. <see cref="TimeSpan.Zero"/> scans on every request, which
    /// is correct but pays a directory walk per request. <see cref="Timeout.InfiniteTimeSpan"/> pins
    /// the snapshot taken at startup and never scans again - the right choice for a tree that is
    /// baked into the deployment and cannot change under the process.
    /// </param>
    /// <remarks>
    /// The scan is driven by requests, so this is an upper bound on how often the disk is touched and
    /// not a background schedule. It is also the window in which a file that changed on disk is still
    /// served from the old snapshot.
    /// </remarks>
    public IoxideFilesBuilder RefreshInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.Zero && interval != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), interval, "The refresh interval must not be negative.");
        }

        _refreshInterval = interval;
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, new IoxideFilesHandler(_assets, new AssetRefresh(_assets, _directory, _refreshInterval, _stamp)));

}
