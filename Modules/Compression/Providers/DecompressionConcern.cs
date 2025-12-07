using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Concern that automatically decompresses incoming request content
/// based on the Content-Encoding header.
/// </summary>
public sealed class DecompressionConcern : IConcern
{
    private const string ContentEncoding = "Content-Encoding";

    #region Get-/Setters

    public IHandler Content { get; }

    private IReadOnlyDictionary<string, IDecompressionAlgorithm> Algorithms { get; }

    #endregion

    #region Initialization

    public DecompressionConcern(IHandler content, IReadOnlyDictionary<string, IDecompressionAlgorithm> algorithms)
    {
        Content = content;
        Algorithms = algorithms;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // Check if request has content and a Content-Encoding header
        if (request.Content != null && request.Headers.TryGetValue(ContentEncoding, out var encoding))
        {
            if (!string.IsNullOrEmpty(encoding))
            {
                // Find matching decompression algorithm
                if (Algorithms.TryGetValue(encoding, out var algorithm))
                {
                    // Wrap the request with decompressed content
                    var decompressedStream = algorithm.Decompress(request.Content);
                    var wrappedRequest = new DecompressedRequest(request, decompressedStream);

                    return await Content.HandleAsync(wrappedRequest);
                }
            }
        }

        // No decompression needed, pass through
        return await Content.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion
}
