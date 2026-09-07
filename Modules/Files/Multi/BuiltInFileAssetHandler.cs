using System.Runtime.CompilerServices;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.FileSystem;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Files.Multi;

internal sealed class BuiltInFileAssetHandler : AbstractAssetsHandler
{
    // Root directory including a trailing separator, so resolving a request is a single
    // concatenation with the (slash-trimmed) relative path - no Path.Combine needed.
    private readonly string _rootPrefix;

    public BuiltInFileAssetHandler(DirectoryInfo directory, List<ICompressionAlgorithm> algorithms, char separator) : base(algorithms, separator)
    {
        var root = directory.FullName;

        _rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FileInfo? ResolveFile(string requestPath)
    {
        var relative = requestPath.AsSpan().TrimStart('/').TrimStart('\\');

        if (relative.IsEmpty)
        {
            return null;
        }

        var file = new FileInfo(string.Concat(_rootPrefix.AsSpan(), relative));

        return file.Exists ? file : null;
    }

}
