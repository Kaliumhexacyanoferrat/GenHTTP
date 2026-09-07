using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using ioxide.file;

namespace GenHTTP.Modules.Files.Multi;

/// <summary>
/// Static-file strategy backed by ioxide.file's <see cref="StaticAssets"/> (an fd cache with baked
/// native responses + statx-based revalidation). GenHTTP frames the status + headers; this writes the
/// body via <see cref="IoxideAssetContent"/>, flushing every &lt;= 12 KB so it never stages more than a
/// slab's worth. GET/HEAD only. Selected by <see cref="FileAssetsHandler"/> on the Ioxide engine.
/// </summary>
internal sealed class IoxideFilesHandler : IHandler
{
    private readonly string _directory;

    private readonly TimeSpan _refreshInterval;

    private readonly PreCompression _preCompression;

    private StaticAssets _assets = null!;

    private AssetRefresh _refresh = null!;

    internal IoxideFilesHandler(string directory, TimeSpan refreshInterval, List<ICompressionAlgorithm> algorithms, char separator)
    {
        _directory = directory;
        _refreshInterval = refreshInterval;
        _preCompression = new PreCompression(algorithms, separator);
    }

    public ValueTask PrepareAsync(IServer server)
    {
        // Opened once and shared across reactors (fds are stable, reads positional); the per-reactor
        // AssetReader pool is resolved lazily in the content. Built here rather than in the builder so
        // the native library is only touched when the server actually runs on the Ioxide engine.
        _assets = new StaticAssets(_directory);

        // Stamped after the snapshot so the two describe the same moment - see the parameter note on
        // the AssetRefresh constructor for what taking it later would cost.
        var stamp = AssetRefresh.Stamp(_directory);

        _refresh = new AssetRefresh(_assets, _directory, _refreshInterval, stamp);

        return default;
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // Throttled: a stat of the tree at most a few times a second, so the snapshot follows the
        // disk without every request paying to ask whether it still matches.
        _refresh.Touch();

        var target = request.Header.Target;

        if (target.HasTrailingSlash)
        {
            return default; // a directory request, not a file (no directory index)
        }

        var path = NormalizePath(target.AsString(decode: true, remainingOnly: true));

        // Resolve + revalidate just to frame Content-Length / Type / Encoding. The body is written later
        // by the content, which re-resolves under its own lease - so no lease is held across the
        // HandleAsync -> WriteAsync boundary (leak-safe for HEAD, where WriteAsync is never called).
        string servePath;
        long length;
        ReadOnlyMemory<byte>? encoding;

        using (var lease = _assets.Acquire())
        {
            // The identity file must exist; precompressed variants are an optimization on top of it.
            if (!lease.TryGet(path, out _))
            {
                return default; // 404 upstream
            }

            if (!request.HasType(RequestMethod.Get, RequestMethod.Head))
            {
                throw new ProviderException(ResponseStatus.MethodNotAllowed, "Only GET and HEAD are allowed for static files", b => b.Header("Allow", "GET, HEAD"));
            }

            // Best accepted precompressed sibling (br > gzip), else the identity file.
            (servePath, encoding) = Negotiate(request, lease, path);

            if (!lease.TryGet(servePath, out var asset))
            {
                return default; // raced away
            }

            // The snapshot is rebuilt when the tree changes, so its length is the size the body
            // writer will produce.
            length = asset.Length;
        }

        // Content-Type from the *identity* file name - the sibling only carries the encoding.
        var contentType = Path.GetFileName(path).GuessContentType() ?? ContentType.ApplicationOctetStream;

        var response = request.Respond()
                              .Content(new IoxideAssetContent(_assets, servePath, length, contentType, encoding));

        if (_preCompression.Enabled)
        {
            response = response.Header("Vary", "Accept-Encoding");
        }

        return new ValueTask<IResponse?>(response.Build());
    }

    // Pick the best precompressed sibling the client accepts (highest priority first), else fall back
    // to identity - the same negotiation the regular handler applies, over the snapshot's siblings.
    private (string Path, ReadOnlyMemory<byte>? Encoding) Negotiate(IRequest request, StaticAssets.Lease lease, string path)
    {
        foreach (var supported in _preCompression.Accepted(request))
        {
            var sibling = path + Encoding.ASCII.GetString(supported.Extension.Span);

            if (lease.TryGet(sibling, out _))
            {
                return (sibling, supported.Algorithm.Name.Bytes);
            }
        }

        return (path, null);
    }

    private static string NormalizePath(string remaining)
        => remaining.Length == 0 || remaining[0] != '/' ? "/" + remaining : remaining;
    
}
