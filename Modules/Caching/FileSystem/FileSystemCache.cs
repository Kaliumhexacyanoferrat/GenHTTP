using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.FileSystem;

public sealed class FileSystemCache<T> : ICache<T>
{
    private static readonly JsonSerializerOptions _Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly SemaphoreSlim _Sync = new(1);

    #region Supporting data structures

    internal record Index(Dictionary<string, string> Entries, Dictionary<string, DateTime> Expiration);

    #endregion

    #region Get-/Setters

    public DirectoryInfo Directory { get; }

    public TimeSpan AccessExpiration { get; }

    #endregion

    #region Initialization

    public FileSystemCache(DirectoryInfo directory, TimeSpan accessExpiration)
    {
            Directory = directory;
            AccessExpiration = accessExpiration;
        }

    #endregion

    #region Functionality

    public async ValueTask<T[]> GetEntriesAsync(string key)
    {
            await _Sync.WaitAsync();

            try
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
            finally
            {
                _Sync.Release();
            }
        }

    public async ValueTask<T?> GetEntryAsync(string key, string variation)
    {
            await _Sync.WaitAsync();

            try
            {
                var index = await GetIndex(key);

                if (index.Entries.TryGetValue(variation, out var fileName))
                {
                    return await GetValue(key, fileName);
                }

                return default;
            }
            finally
            {
                _Sync.Release();
            }
        }

    public async ValueTask StoreAsync(string key, string variation, T? entry)
    {
            await _Sync.WaitAsync();

            try
            {
                var index = await GetIndex(key);

                EnsureDirectory(key);

                if (index.Entries.TryGetValue(variation, out var fileName))
                {
                    index.Expiration.Add(fileName, DateTime.UtcNow);

                    if (entry == null)
                    {
                        index.Entries.Remove(variation);
                    }
                    else
                    {
                        fileName = Guid.NewGuid().ToString();

                        index.Entries[variation] = fileName;

                        await StoreValue(key, fileName, entry);
                    }

                    await StoreIndex(key, index);
                }
                else if (entry != null)
                {
                    var newFile = Guid.NewGuid().ToString();

                    index.Entries.Add(variation, newFile);

                    await StoreValue(key, newFile, entry);

                    await StoreIndex(key, index);
                }

            }
            finally
            {
                _Sync.Release();
            }
        }

    public async ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter)
    {
            await _Sync.WaitAsync();

            try
            {
                var index = await GetIndex(key);

                EnsureDirectory(key);

                if (index.Entries.TryGetValue(variation, out var fileName))
                {
                    index.Expiration.Add(fileName, DateTime.UtcNow);

                    fileName = Guid.NewGuid().ToString();

                    index.Entries[variation] = fileName;

                    var file = new FileInfo(Path.Combine(Directory.FullName, key, fileName));

                    using (var streamWriter = new StreamWriter(file.FullName, false))
                    {
                        await asyncWriter(streamWriter.BaseStream);
                    }

                    await StoreIndex(key, index);
                }
                else
                {
                    var newFile = Guid.NewGuid().ToString();

                    index.Entries.Add(variation, newFile);

                    var file = new FileInfo(Path.Combine(Directory.FullName, key, newFile));

                    using (var stream = file.OpenWrite())
                    {
                        await asyncWriter(stream);
                    }

                    await StoreIndex(key, index);
                }
            }
            finally
            {
                _Sync.Release();
            }
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

                    return (await JsonSerializer.DeserializeAsync<T>(stream, _Options));
                }
            }

            return default;
        }

    private async ValueTask StoreValue(string key, string fileName, T value)
    {
            var file = new FileInfo(Path.Combine(Directory.FullName, key, fileName));

            using var writer = new StreamWriter(file.FullName, false);

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                if (value is Stream source)
                {
                    await source.CopyToAsync(writer.BaseStream);
                }
            }
            else
            {
                await JsonSerializer.SerializeAsync(writer.BaseStream, value, _Options);
            }
        }

    private async ValueTask<Index> GetIndex(string key)
    {
            var indexFile = new FileInfo(Path.Combine(Directory.FullName, key, "index.json"));

            if (indexFile.Exists)
            {
                using var stream = indexFile.OpenRead();

                return (await JsonSerializer.DeserializeAsync<Index>(stream, _Options)) ?? new Index(new(), new());
            }

            return new Index(new(), new());
        }

    private async ValueTask StoreIndex(string key, Index index)
    {
            RunHouseKeeping(key, index);

            var indexFile = new FileInfo(Path.Combine(Directory.FullName, key, "index.json"));

            using (var writer = new StreamWriter(indexFile.FullName, false))
            {
                await JsonSerializer.SerializeAsync(writer.BaseStream, index, _Options);
            }

            indexFile.Refresh();
        }

    private void RunHouseKeeping(string key, Index index)
    {
            var toDelete = new List<string>();

            foreach (var entry in index.Expiration)
            {
                if (entry.Value < DateTime.UtcNow.Add(-AccessExpiration))
                {
                    toDelete.Add(entry.Key);
                }
            }

            foreach (var file in toDelete)
            {
                try
                {
                    Remove(key, file);

                    index.Expiration.Remove(file);
                }
                catch
                {
                    // probably still accessed
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

    private void EnsureDirectory(string key)
    {
            var subPath = new DirectoryInfo(Path.Combine(Directory.FullName, key));

            if (!subPath.Exists)
            {
                subPath.Create();
            }
        }

    #endregion

}
