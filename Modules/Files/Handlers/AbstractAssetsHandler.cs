using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Compression.Providers;

namespace GenHTTP.Modules.Files.Handlers;

public abstract class AbstractAssetsHandler : IHandler
{
    private readonly List<SupportedCompression> _algorithms;

    protected AbstractAssetsHandler(List<ICompressionAlgorithm> algorithms, char separator)
    {
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

    public ValueTask PrepareAsync() => default;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.HasTrailingSlash)
        {
            return null;
        }

        if (_algorithms.Count > 0)
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
            return request.Respond()
                          .Content(content)
                          .Build();
        }

        return null;
    }

    private async ValueTask<IResponse?> TryGetPreCompressed(IRequest request)
    {
        var target = request.Header.Target;

        var acceptEncodingHeader = request.Header.Headers.GetEntry(KnownHeaders.AcceptEncoding);

        if (acceptEncodingHeader != null)
        {
            var requested = AcceptEncodingHeader.ParseSupported(acceptEncodingHeader.Value.Span);

            foreach (var supported in _algorithms)
            {
                if (requested.Contains(supported.Algorithm.Name))
                {
                    var newTarget = target.CopyAndAppend(supported.Extension);

                    var content = await Resolve(newTarget);

                    if (content != null)
                    {
                        return request.Respond()
                                      .Content(content)
                                      .Build();
                    }
                }
            }
        }

        return null;
    }

    protected abstract ValueTask<IResponseContent?> Resolve(IRequestTarget target);

}
