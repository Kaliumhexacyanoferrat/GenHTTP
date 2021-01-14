using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.FileSystem
{

    public class FileSystemCache<T> : ICache<T>, IDisposable
    {

        #region Supporting data structures

        internal record Index(Dictionary<string, string> Entries);

        #endregion

        #region Get-/Setters

        public DirectoryInfo Directory { get; }

        #endregion

        #region Initialization

        public FileSystemCache(DirectoryInfo directory)
        {
            Directory = directory;
        }

        #endregion

        #region Functionality

        public async ValueTask<T[]> GetEntriesAsync(string key)
        {
            var result = new List<T>();

            var index = await GetIndex(key);

            foreach (var entry in index.Entries)
            {
                var value = await GetValue(key, entry.Value);

                if (value != null)
                {
                    result.Add(value);
                }
            }

            return result.ToArray();
        }

        public async ValueTask<T?> GetEntryAsync(string key, string variation)
        {
            var index = await GetIndex(key);

            if (index.Entries.TryGetValue(variation, out var fileName))
            {
                return await GetValue(key, fileName);
            }

            return default;
        }

        private async ValueTask<T?> GetValue(string key, string fileName)
        {
            var file = new FileInfo(Path.Combine(Directory.FullName, key, fileName));

            if (file.Exists)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)file.OpenRead();
                }
                else
                {
                    using var stream = file.OpenRead();

                    return (await JsonSerializer.DeserializeAsync<T>(stream));
                }
            }

            return default;
        }

        public async ValueTask StoreAsync(string key, string variation, T? entry)
        {
            var index = await GetIndex(key);

            EnsureDirectory(key);

            if (index.Entries.TryGetValue(variation, out var fileName))
            {
                if (entry == null)
                {
                    Remove(key, fileName);

                    index.Entries.Remove(variation);

                    await StoreIndex(key, index);
                }
                else
                {
                    await StoreValue(key, fileName, entry);
                }
            }
            else if (entry != null)
            {
                var newFile = Guid.NewGuid().ToString();

                index.Entries.Add(variation, newFile);

                await StoreValue(key, newFile, entry);

                await StoreIndex(key, index);
            }
        }

        public async ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter)
        {
            var index = await GetIndex(key);

            EnsureDirectory(key);

            if (index.Entries.TryGetValue(variation, out var fileName))
            {
                var file = new FileInfo(Path.Combine(Directory.FullName, key, fileName));

                using var target = (file.Exists) ? file.OpenWrite() : file.Create();

                await asyncWriter(target);
            }
            else
            {
                var newFile = Guid.NewGuid().ToString();

                index.Entries.Add(variation, newFile);

                var file = new FileInfo(Path.Combine(Directory.FullName, key, newFile));

                using var target = (file.Exists) ? file.OpenWrite() : file.Create();

                await asyncWriter(target);

                await StoreIndex(key, index);
            }
        }

        private async ValueTask StoreValue(string key, string fileName, T value)
        {
            var file = new FileInfo(Path.Combine(Directory.FullName, key, fileName));

            using (var writer = new StreamWriter(file.FullName, false))
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    var source = value as Stream;

                    if (source != null)
                    {
                        await source.CopyToAsync(writer.BaseStream);
                    }
                }
                else
                {
                    await JsonSerializer.SerializeAsync(writer.BaseStream, value);
                }
            }
        }

        private void Remove(string key, string fileName)
        {
            var file = new FileInfo(Path.Combine(Directory.FullName, key, fileName));

            if (file.Exists)
            {
                file.Delete();
            }
        }

        private async ValueTask<Index> GetIndex(string key)
        {
            var indexFile = new FileInfo(Path.Combine(Directory.FullName, key, "index.json"));

            if (indexFile.Exists)
            {
                using var stream = indexFile.OpenRead();

                return (await JsonSerializer.DeserializeAsync<Index>(stream)) ?? new Index(new());
            }

            return new Index(new());
        }

        private async ValueTask StoreIndex(string key, Index index)
        {
            var indexFile = new FileInfo(Path.Combine(Directory.FullName, key, "index.json"));

            using (var writer = new StreamWriter(indexFile.FullName, false)) 
            { 
                await JsonSerializer.SerializeAsync(writer.BaseStream, index);
            }

            indexFile.Refresh();
        }

        private void EnsureDirectory(string key)
        {
            var subPath = new DirectoryInfo(Path.Combine(Directory.FullName, key));

            if (!subPath.Exists)
            {
                subPath.Create();
            }
        }

        #endregion

        #region Disposing

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Directory.Delete(true);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
