using ioxide.file;

namespace GenHTTP.Modules.Files.Multi;

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
/// The walk is driven by requests and nothing else. There is no background timer, so a mount that is
/// not being asked for is not being walked, and a tree that never changes costs one stat sweep per
/// interval only for as long as traffic keeps arriving.
///
/// The stamp is the file count, the newest write time and the total size. Size alone is not enough:
/// an atomic replace (write a temporary, rename it over the target) can leave a file of exactly the
/// same length, and anything keyed on size would serve the old bytes forever. Write time alone is
/// not enough either, since an edit can land inside the filesystem's timestamp granularity. Each
/// covers what the other misses, so the stamp carries both.
/// </summary>
internal sealed class AssetRefresh
{
    /// <summary>
    /// Well inside the two seconds a caller replacing a file would expect, and long enough that the
    /// stat cost disappears against the request rate.
    /// </summary>
    internal static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(250);

    private readonly StaticAssets _assets;
    private readonly string? _root;
    private readonly long _intervalTicks;

    private long _nextCheck;
    private long _stamp;
    private int _busy;

    /// <param name="stamp">
    /// The tree as it was when <paramref name="assets"/> was snapshotted. Passed in rather than taken
    /// here because the two have to describe the same moment: taken later, an edit made in between
    /// would already be in the stamp, the first check would find nothing to do, and the snapshot
    /// would keep describing the tree as it was before the edit.
    /// </param>
    internal AssetRefresh(StaticAssets assets, string root, TimeSpan interval, long stamp)
    {
        _assets = assets;

        // Timeout.InfiniteTimeSpan pins the snapshot: no walk, ever. Treated the same way as a root
        // that is not there, so the check is one null test on the hot path either way.
        _root = interval != Timeout.InfiniteTimeSpan && Directory.Exists(root) ? root : null;

        _intervalTicks = interval == Timeout.InfiniteTimeSpan ? 0 : interval.Ticks;

        _stamp = stamp;

        // Deliberately not now + interval: the snapshot is taken in the builder, and the tree can be
        // edited between that and the first request. Leaving the first check due means request one
        // pays a single walk and sees the edit; the throttle applies from then on.
        _nextCheck = 0;
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
            Interlocked.Exchange(ref _nextCheck, now + _intervalTicks);

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

    // File count, newest write time and total size. The write time is what catches a replacement of
    // identical length; the size is what catches an edit that lands inside the filesystem's timestamp
    // granularity. Either alone misses a case the other sees, so the stamp carries both.
    private long Stamp() => Stamp(_root);

    internal static long Stamp(string? root)
    {
        if (root is null || !Directory.Exists(root))
        {
            return 0;
        }

        long count = 0;
        long newest = 0;
        long bytes = 0;

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            count++;

            var info = new FileInfo(path);

            var written = info.LastWriteTimeUtc.Ticks;

            if (written > newest)
            {
                newest = written;
            }

            bytes += info.Length;
        }

        return HashCode.Combine(count, newest, bytes);
    }
}
