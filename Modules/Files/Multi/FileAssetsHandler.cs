using fdout;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Files.Serving;

namespace GenHTTP.Modules.Files.Multi;

public sealed class FileAssetsHandler : AbstractAssetsHandler, IDisposable
{
    private readonly RandomAccessCache _cache;

    public FileAssetsHandler(DirectoryInfo directory, List<ICompressionAlgorithm> algorithms, char separator) : base(algorithms, separator)
    {
        _cache = new RandomAccessCache(directory.FullName);
    }

    protected override ValueTask<IResponseContent?> Resolve(IRequestTarget target, ContentType? contentType = null, ReadOnlyMemory<byte>? contentEncoding = null)
    {
        var path = target.AsString(decode: true, remainingOnly: true);

        var normalized = Normalize(path);

        if (_cache.TryGet(normalized, out var entry))
        {
            return new(new EntryResponseContent(_cache, entry, contentType, contentEncoding));
        }

        return default;
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    private static string Normalize(string path)
    {
        var needsSlash = path.Length == 0 || path[0] != '/';
        var needsReplace = path.Contains('\\');

        if (!needsSlash && !needsReplace)
        {
            return path;
        }

        return (needsSlash ? "/" : "") + path.Replace('\\', '/');
    }

}
