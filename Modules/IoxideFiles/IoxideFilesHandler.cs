using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression.Providers;
using GenHTTP.Modules.IO;

using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Resolves a request path against the shared <see cref="StaticAssets"/> cache, frames status +
/// headers (Content-Type from the identity file, plus precompressed negotiation), and hands the body
/// off to an <see cref="IoxideAssetContent"/>. GET/HEAD only.
/// </summary>
public sealed class IoxideFilesHandler : IHandler
{
    // Precompressed negotiation: the request's Accept-Encoding is matched against these, and the
    // ".br"/".gz" sibling served (with the matching Content-Encoding) when present. br before gzip.
    private static readonly AlgorithmName Brotli = new("br"u8.ToArray());
    private static readonly AlgorithmName Gzip = new("gzip"u8.ToArray());

    private readonly StaticAssets _assets;

    internal IoxideFilesHandler(StaticAssets assets)
    {
        _assets = assets;
    }

    public ValueTask PrepareAsync(IServer server) => default;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
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

            if (AssetCache.IsFresh(asset, out var exists, out var currentSize))
            {
                length = asset.Length;
            }
            else if (exists)
            {
                length = currentSize; // changed on disk since the snapshot - serve the current size
            }
            else
            {
                return default; // vanished -> 404
            }
        }

        // Content-Type from the *identity* file name - the sibling only carries the encoding.
        var contentType = Path.GetFileName(path).GuessContentType() ?? ContentType.ApplicationOctetStream;

        var response = request.Respond()
                              .Header("Vary", "Accept-Encoding")
                              .Content(new IoxideAssetContent(_assets, servePath, length, contentType, encoding));

        return new ValueTask<IResponse?>(response.Build());
    }

    // Pick the best precompressed sibling the client accepts (br before gzip), else fall back to identity.
    private static (string Path, ReadOnlyMemory<byte>? Encoding) Negotiate(IRequest request, StaticAssets.Lease lease, string path)
    {
        var header = request.Header.Headers.GetEntry(KnownHeaders.AcceptEncoding);

        if (header != null)
        {
            var accepted = AcceptEncodingHeader.ParseSupported(header.Value);

            if (accepted.Contains(Brotli) && lease.TryGet(path + ".br", out _))
            {
                return (path + ".br", Brotli.Bytes);
            }

            if (accepted.Contains(Gzip) && lease.TryGet(path + ".gz", out _))
            {
                return (path + ".gz", Gzip.Bytes);
            }
        }

        return (path, null);
    }

    private static string NormalizePath(string remaining)
        => remaining.Length == 0 || remaining[0] != '/' ? "/" + remaining : remaining;
}
