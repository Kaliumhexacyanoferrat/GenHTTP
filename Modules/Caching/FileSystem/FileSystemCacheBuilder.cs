using System.IO;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Caching.FileSystem
{

    public class FileSystemCacheBuilder<T> : IBuilder<FileSystemCache<T>>
    {
        private DirectoryInfo? _Directory;

        #region Functionality

        public FileSystemCacheBuilder<T> Directory(DirectoryInfo directory)
        {
            _Directory = directory;
            return this;
        }

        public FileSystemCache<T> Build()
        {
            var directory = _Directory ?? throw new BuilderMissingPropertyException("Directory");

            if (!directory.Exists)
            {
                directory.Create();
            }

            return new FileSystemCache<T>(directory);
        }

        #endregion

    }

}
