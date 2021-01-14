using System;
using System.IO;

using GenHTTP.Modules.Caching.FileSystem;
using GenHTTP.Modules.Caching.Memory;

namespace GenHTTP.Modules.Caching
{

    public static class Cache
    {

        public static IMemoryCacheBuilder<T> Memory<T>()
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return new StreamMemoryCacheBuilder<T>();
            }
            else
            {
                return new MemoryCacheBuilder<T>();
            }
        }

        public static FileSystemCacheBuilder<T> FileSystem<T>(DirectoryInfo directory)
            => new FileSystemCacheBuilder<T>().Directory(directory);

        public static FileSystemCacheBuilder<T> FileSystem<T>(string directory)
            => FileSystem<T>(new DirectoryInfo(directory));

        public static FileSystemCacheBuilder<T> TemporaryFiles<T>()
            => FileSystem<T>(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));

    }

}
