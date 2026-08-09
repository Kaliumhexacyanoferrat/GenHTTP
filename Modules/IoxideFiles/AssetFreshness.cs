using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Whether a snapshot's asset still matches the file on disk, by size - the same check
/// ioxide.file performed until 0.4.167, when it moved to trusting descriptors for the lifetime of
/// a snapshot. Reproduced here so this module keeps serving edited files without a reload.
/// </summary>
internal static class AssetFreshness
{

    /// <summary>
    /// True when the descriptor can be trusted. <paramref name="exists"/> and
    /// <paramref name="currentLength"/> describe the file as it is now, so a caller that gets
    /// false can still serve the changed file rather than 404 it.
    /// </summary>
    internal static bool IsFresh(in AssetCache.Asset asset, out bool exists, out long currentLength)
    {
        try
        {
            var info = new FileInfo(asset.Path);

            exists = info.Exists;
            currentLength = exists ? info.Length : 0;

            return exists && currentLength == asset.Length;
        }
        catch (IOException)
        {
            // Racing with a rename or delete: treat as gone rather than serve a stale descriptor.
            exists = false;
            currentLength = 0;

            return false;
        }
        catch (UnauthorizedAccessException)
        {
            exists = false;
            currentLength = 0;

            return false;
        }
    }

}
