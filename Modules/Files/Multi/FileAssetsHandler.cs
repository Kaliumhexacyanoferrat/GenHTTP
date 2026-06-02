using fdout;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Files.Serving;

namespace GenHTTP.Modules.Files.Multi;

public sealed class FileAssetsHandler : AbstractAssetsHandler
{
    private readonly DirectoryInfo _directory;

    private readonly RandomAccessCache _cache;

    public FileAssetsHandler(DirectoryInfo directory, List<ICompressionAlgorithm> algorithms, char separator) : base(algorithms, separator)
    {
        _directory = directory;

        _cache = new RandomAccessCache(_directory.FullName);
    }

    protected override ValueTask<IResponseContent?> Resolve(IRequestTarget target, ContentType? contentType = null, ReadOnlyMemory<byte>? contentEncoding = null)
    {
        var path = target.AsString(decode: true, remainingOnly: true);

        var targetFile = Path.Combine(_directory.FullName, path);

        if (_cache.TryGet(targetFile, out var entry))
        {
            return new(new EntryResponseContent(_cache, entry, contentType, contentEncoding));
        }

        return default;
    }

}
