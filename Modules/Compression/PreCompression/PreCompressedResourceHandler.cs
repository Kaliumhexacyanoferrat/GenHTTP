using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Compression.Providers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Compression.PreCompression;

public sealed class PreCompressedResourceHandler : IHandler
{
    private readonly IResourceTree _tree;

    private readonly IHandler _regular;

    private readonly List<ICompressionAlgorithm> _algorithms;

    public PreCompressedResourceHandler(IResourceTree tree, List<ICompressionAlgorithm> algorithms)
    {
        _tree = tree;
        _algorithms = algorithms;

        _regular = Resources.From(tree).Build();
    }

    public ValueTask PrepareAsync() => _regular.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        var file = GetFileName(target);

        if (file is null)
        {
            return null;
        }

        var acceptHeader = request.Header.Headers.GetEntry(KnownHeaders.Accept);

        if (acceptHeader != null)
        {
            var supported = AcceptHeader.ParseSupported(acceptHeader.Value.Span);

            foreach (var algorithm in _algorithms.OrderByDescending(a => (int)a.Priority))
            {
                if (supported.Contains(algorithm.Name))
                {
                    var extension = new byte[algorithm.Name.Value.Length + 1];
                    extension[0] = (byte)'.';
                    algorithm.Name.Value.Span.CopyTo(extension.AsSpan(1));

                    var newTarget = target.CopyAndAppend(extension);

                    var (_, resource) = await _tree.FindAsync(newTarget);

                    if (resource is not null)
                    {
                        var contentType = file.GuessContentType() ?? ContentType.ApplicationOctetStream;
                        var content = new ResourceContent(resource, contentType, algorithm.Name.Value);

                        return request.Respond()
                                      .Content(content)
                                      .Build();
                    }
                }
            }
        }

        return await _regular.HandleAsync(request);
    }

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
