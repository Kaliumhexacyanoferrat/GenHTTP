using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.FileSystem;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Files.Multi;

internal sealed class BuiltInFileAssetHandler : AbstractAssetsHandler
{
    private readonly string _root;

    public BuiltInFileAssetHandler(DirectoryInfo directory, List<ICompressionAlgorithm> algorithms, char separator) : base(algorithms, separator)
    {
        _root = directory.FullName;
    }

    protected override ValueTask<IResponseContent?> Resolve(IRequestTarget target, ContentType? contentType = null, ReadOnlyMemory<byte>? contentEncoding = null)
    {
        var path = target.AsString(decode: true, remainingOnly: true);

        var file = ResolveFile(path);

        if (file is not null)
        {
            var resource = new FileResource(file, file.Name, contentType);

            return new(new ResourceContent(resource, contentType, contentEncoding));
        }

        return default;
    }

    private FileInfo? ResolveFile(string requestPath)
    {
        var relative = requestPath.TrimStart('/', '\\');

        if (relative.Length == 0)
        {
            return null;
        }

        var full = Path.GetFullPath(Path.Combine(_root, relative));

        var file = new FileInfo(full);

        return file.Exists ? file : null;
    }

}
