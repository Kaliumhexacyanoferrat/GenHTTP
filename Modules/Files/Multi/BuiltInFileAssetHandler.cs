using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.FileSystem;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Files.Multi;

internal sealed class BuiltInFileAssetHandler : AbstractAssetsHandler
{
    private readonly string _rootPrefix;

    public BuiltInFileAssetHandler(DirectoryInfo directory, List<ICompressionAlgorithm> algorithms, char separator) : base(algorithms, separator)
    {
        var root = directory.FullName;

        _rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
    }

    protected override ValueTask<IResponseContent?> Resolve(IRequestTarget target, ContentType? contentType = null, ReadOnlyMemory<byte>? contentEncoding = null)
    {
        var file = ResolveFile(target);

        if (file is not null)
        {
            var resource = new FileResource(file, file.Name, contentType);

            return new(new ResourceContent(resource, contentType, contentEncoding));
        }

        return default;
    }

    private FileInfo? ResolveFile(IRequestTarget target)
    {
        var length = _rootPrefix.Length;
        var segments = 0;

        PathSegment? segment;

        while ((segment = target.Next(segments)) != null)
        {
            length += segment.Value.Bytes.Length + 1;
            segments++;
        }

        if (segments == 0)
        {
            return null;
        }

        var buffer = length <= 1024 ? stackalloc char[length] : new char[length];

        _rootPrefix.CopyTo(buffer);

        var written = _rootPrefix.Length;
        var index = 0;

        while ((segment = target.Next(index)) != null)
        {
            if (index++ > 0)
            {
                buffer[written++] = Path.DirectorySeparatorChar;
            }

            written += segment.Value.Decode(buffer[written..]);
        }

        var file = new FileInfo(new string(buffer[..written]));

        if (!file.FullName.StartsWith(_rootPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        return file.Exists ? file : null;
    }

}
