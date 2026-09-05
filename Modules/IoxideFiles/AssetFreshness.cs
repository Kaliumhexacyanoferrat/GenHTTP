using Microsoft.Win32.SafeHandles;

using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Whether a snapshot's asset still matches the file on disk, so this module keeps serving edited
/// files without a reload.
///
/// Size alone is not enough, and the case it misses is the common one: an atomic replace (write a
/// temporary, rename it over the target) leaves a NEW inode behind the same path, while the
/// snapshot's descriptor still refers to the old one. Nothing about the size need change, so a
/// size check calls it fresh and the old contents are served indefinitely. Editing a file IN
/// PLACE is the case size does not need to catch at all - the descriptor already sees those bytes.
///
/// So the descriptor is compared against the path: same size, and the same last-write time as the
/// file the path resolves to now. A rename puts a different file there, and a different file has
/// a different timestamp.
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

            if (!exists || currentLength != asset.Length)
            {
                return false;
            }

            // Borrowed, not owned: the snapshot closes this descriptor when it is disposed.
            using var handle = new SafeFileHandle((nint)asset.Fd, ownsHandle: false);

            return File.GetLastWriteTimeUtc(handle) == info.LastWriteTimeUtc;
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
