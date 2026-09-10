using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Files.Multi;

public abstract class AbstractAssetsHandler : IHandler
{

    private readonly PreCompression _preCompression;

    protected AbstractAssetsHandler(List<ICompressionAlgorithm> algorithms, char separator)
    {
        _preCompression = new PreCompression(algorithms, separator);
    }

    public ValueTask PrepareAsync(IServer server) => default;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;
        
        if (target.HasTrailingSlash)
        {
            return null;
        }

        target.DenyPathTraversal();
        
        if (_preCompression.Enabled)
        {
            var handled = await TryGetPreCompressed(request);

            if (handled != null)
            {
                return handled;
            }
        }

        var content = await Resolve(target);

        if (content != null)
        {
            var response = request.Respond().Content(content);

            // The identity response still varies by Accept-Encoding whenever a compressed variant
            // could have been served to a different client.
            if (_preCompression.Enabled)
            {
                response = response.Header("Vary", "Accept-Encoding");
            }

            return response.Build();
        }

        return null;
    }

    private async ValueTask<IResponse?> TryGetPreCompressed(IRequest request)
    {
        var target = request.Header.Target;

        foreach (var supported in _preCompression.Accepted(request))
        {
            var newTarget = target.CopyAndAppend(supported.Extension);

            var fileName = GetFileName(target);

            var contentType = fileName?.GuessContentType() ?? ContentType.ApplicationOctetStream;

            var content = await Resolve(newTarget, contentType, supported.Algorithm.Name.Bytes);

            if (content != null)
            {
                return request.Respond()
                              .Content(content)
                              .Header("Vary", "Accept-Encoding")
                              .Build();
            }
        }

        return null;
    }

    protected abstract ValueTask<IResponseContent?> Resolve(IRequestTarget target, ContentType? contentType = null, ReadOnlyMemory<byte>? contentEncoding = null);

    private static string? GetFileName(IRequestTarget target)
    {
        if (target.HasTrailingSlash)
        {
            return null;
        }

        var offset = 0;

        PathSegment? current = null;

        while (true)
        {
            var next = target.Next(offset++);

            if (next == null)
            {
                break;
            }

            current = next.Value;
        }

        return current?.Decode();
    }

}
