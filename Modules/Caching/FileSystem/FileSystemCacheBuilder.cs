using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Caching.FileSystem;

public class FileSystemCacheBuilder<T> : IBuilder<FileSystemCache<T>>
{

    private TimeSpan _AccessExpiration = TimeSpan.FromMinutes(30);
    private DirectoryInfo? _Directory;

    #region Functionality

    public FileSystemCacheBuilder<T> Directory(DirectoryInfo directory)
    {
        _Directory = directory;
        return this;
    }

    /// <summary>
    /// Sets the duration old files will be kept to allow clients to finish
    /// their downloads.
    /// </summary>
    /// <param name="expiration"></param>
    /// <returns></returns>
    /// <remarks>
    /// Defaults to 30 minutes. If you serve very large files or your clients
    /// download very slow, consider to increase this value..
    /// </remarks>
    public FileSystemCacheBuilder<T> AccessExpiration(TimeSpan expiration)
    {
        _AccessExpiration = expiration;
        return this;
    }

    public FileSystemCache<T> Build()
    {
        var directory = _Directory ?? throw new BuilderMissingPropertyException("Directory");

        if (!directory.Exists)
        {
            directory.Create();
        }

        return new FileSystemCache<T>(directory, _AccessExpiration);
    }

    #endregion

}
