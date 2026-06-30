using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Concern that automatically decompresses incoming request content
/// based on the Content-Encoding header.
/// </summary>
public sealed class DecompressionConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private IReadOnlyDictionary<AlgorithmName, ICompressionAlgorithm> Algorithms { get; }

    #endregion

    #region Initialization

    public DecompressionConcern(IHandler content, IReadOnlyDictionary<AlgorithmName, ICompressionAlgorithm> algorithms)
    {
        Content = content;
        Algorithms = algorithms;
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var encoding = request.Header.Headers.GetEntry(KnownHeaders.ContentEncoding);

        if (encoding != null)
        {
            var requestedAlgorithm = new AlgorithmName(encoding.Value.Bytes);

            if (Algorithms.TryGetValue(requestedAlgorithm, out var algorithm))
            {
                request.WrapBody(b => new DecompressedBody(b, algorithm));
            }
        }

        return Content.HandleAsync(request);
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    #endregion

}
