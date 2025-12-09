using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
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

    private IReadOnlyDictionary<string, ICompressionAlgorithm> Algorithms { get; }

    #endregion

    #region Initialization

    public DecompressionConcern(IHandler content, IReadOnlyDictionary<string, ICompressionAlgorithm> algorithms)
    {
        Content = content;
        Algorithms = algorithms;
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Content != null && request.Headers.TryGetValue(ContentEncoding, out var encoding))
        {
            if (!string.IsNullOrEmpty(encoding))
            {
                if (Algorithms.TryGetValue(encoding, out var algorithm))
                {
                    using var decompressedContent = algorithm.Decompress(request.Content);

                    using var wrappedRequest = new WrappedRequest(request, decompressedContent);

                    return Content.HandleAsync(wrappedRequest);
                }
            }
        }

        return Content.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
