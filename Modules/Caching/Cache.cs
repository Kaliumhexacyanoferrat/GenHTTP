using GenHTTP.Modules.Caching.FileSystem;
using GenHTTP.Modules.Caching.Memory;

namespace GenHTTP.Modules.Caching;

/// <summary>
/// Creates caches with different kind of backends.
/// </summary>
public static class Cache
{

    /// <summary>
    /// A cache that will store all data in memory.
    /// </summary>
    /// <typeparam name="T">The type of the entries to be stored</typeparam>
    public static IMemoryCacheBuilder<T> Memory<T>()
    {
        if (typeof(Stream).IsAssignableFrom(typeof(T)))
        {
            return new StreamMemoryCacheBuilder<T>();
        }
        return new MemoryCacheBuilder<T>();
    }

    /// <summary>
    /// A cache that will store all data in the specified directory.
    /// </summary>
    /// <typeparam name="T">The type of the entries to be stored</typeparam>
    /// <param name="directory">The directory to store the entries in</param>
    public static FileSystemCacheBuilder<T> FileSystem<T>(DirectoryInfo directory)
        => new FileSystemCacheBuilder<T>().Directory(directory);

    /// <summary>
    /// A cache that will store all data in the specified directory.
    /// </summary>
    /// <typeparam name="T">The type of the entries to be stored</typeparam>
    /// <param name="directory">The directory to store the entries in</param>
    public static FileSystemCacheBuilder<T> FileSystem<T>(string directory)
        => FileSystem<T>(new DirectoryInfo(directory));

    /// <summary>
    /// A cache that will use files placed in the temp directory of the
    /// operating system to store entries.
    /// </summary>
    /// <typeparam name="T">The type of the entries to be stored</typeparam>
    public static FileSystemCacheBuilder<T> TemporaryFiles<T>()
        => FileSystem<T>(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));
}
