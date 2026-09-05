using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Keeps the snapshot honest without paying for it per request.
///
/// The snapshot is built once: a descriptor per file, read positionally off the ring. That is what
/// makes it fast, and it is also why replacing a file underneath the mount would otherwise keep
/// serving the bytes the process opened at startup.
///
/// So the tree is stamped instead, and the stamp is checked before a static request is answered -
/// throttled, so a saturating run stats the directory a few times a second rather than a few
/// hundred thousand. When the stamp moves, the whole snapshot is rebuilt.
///
/// The stamp is the file count plus the newest write time. Size is deliberately not part of it: an
/// atomic replace (write a temporary, rename it over the target) can leave a file of exactly the
/// same length, and anything keyed on size alone serves the old bytes forever.
/// </summary>
internal sealed class AssetRefresh
{
    // Well inside the two seconds a caller replacing a file would expect, and long enough that the
    // stat cost disappears against the request rate.
    private static readonly long IntervalTicks = TimeSpan.TicksPerMillisecond * 250;

    private readonly StaticAssets _assets;
    private readonly string? _root;

    private long _nextCheck;
    private long _stamp;
    private int _busy;

    internal AssetRefresh(StaticAssets assets, string root)
    {
        _assets = assets;
        _root = Directory.Exists(root) ? root : null;

        _stamp = Stamp();
        _nextCheck = DateTime.UtcNow.Ticks + IntervalTicks;
    }

    /// <summary>Rebuild the snapshot if the tree changed, at most once per interval.</summary>
    internal void Touch()
    {
        if (_root is null)
        {
            return;
        }

        var now = DateTime.UtcNow.Ticks;

        if (now < Interlocked.Read(ref _nextCheck))
        {
            return;
        }

        // One reactor does the walk; the rest carry on serving from the snapshot they have.
        if (Interlocked.Exchange(ref _busy, 1) == 1)
        {
            return;
        }

        try
        {
            Interlocked.Exchange(ref _nextCheck, now + IntervalTicks);

            var stamp = Stamp();

            if (stamp != Interlocked.Read(ref _stamp))
            {
                Interlocked.Exchange(ref _stamp, stamp);
                _assets.Reload();
            }
        }
        catch (IOException)
        {
            // The tree moved while it was being walked; the next interval will see it settled.
        }
        catch (UnauthorizedAccessException)
        {
        }
        finally
        {
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    // File count and the newest write time, which together move whenever a file is added, removed
    // or replaced - including a replacement of identical length.
    private long Stamp()
    {
        if (_root is null)
        {
            return 0;
        }

        long count = 0;
        long newest = 0;

        foreach (var path in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
        {
            count++;

            var written = File.GetLastWriteTimeUtc(path).Ticks;

            if (written > newest)
            {
                newest = written;
            }
        }

        return (count * 31) ^ newest;
    }
}
