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

    private readonly List<SupportedCompression> _algorithms;

    public PreCompressedResourceHandler(IResourceTree tree, List<ICompressionAlgorithm> algorithms, char separator)
    {
        _tree = tree;

        _regular = Resources.From(tree).Build();

        _algorithms = algorithms.Select(a =>
                                {
                                    var extension = new byte[a.Name.Value.Length + 1];
                                    extension[0] = (byte)separator;
                                    a.Name.Value.Span.CopyTo(extension.AsSpan(1));
                                    
                                    return new SupportedCompression(a, extension);
                                })
                                .OrderByDescending(a => (int)a.Algorithm.Priority)
                                .ToList();
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

        var acceptEncodingHeader = request.Header.Headers.GetEntry(KnownHeaders.AcceptEncoding);

        if (acceptEncodingHeader != null)
        {
            var requested = AcceptEncodingHeader.ParseSupported(acceptEncodingHeader.Value.Span);

            foreach (var supported in _algorithms)
            {
                if (requested.Contains(supported.Algorithm.Name))
                {
                    var newTarget = target.CopyAndAppend(supported.Extension);

                    var (_, resource) = await _tree.FindAsync(newTarget);

                    if (resource is not null)
                    {
                        var contentType = file.GuessContentType() ?? ContentType.ApplicationOctetStream;
                        var content = new ResourceContent(resource, contentType, supported.Algorithm.Name.Value);

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
