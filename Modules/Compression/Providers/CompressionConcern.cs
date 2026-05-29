using System.IO.Compression;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressionConcern : IConcern
{
    private static readonly HashSet<ContentType> CompressibleTypes =
    [
        ContentType.ApplicationJavaScript,
        ContentType.ApplicationJson,
        ContentType.AudioWav,
        ContentType.TextCss,
        ContentType.TextCsv,
        ContentType.TextHtml,
        ContentType.TextPlain,
        ContentType.TextRichText,
        ContentType.FontWoff,
        ContentType.FontTrueTypeFont,
        ContentType.FontOpenTypeFont,
        ContentType.FontEmbeddedOpenTypeFont,
        ContentType.ImageScalableVectorGraphics,
        ContentType.ImageScalableVectorGraphicsXml,
        ContentType.ImageBmp,
        ContentType.TextXml,
        ContentType.TextJavaScript,
        ContentType.ApplicationYaml
    ];

    #region Get-/Setters

    public IHandler Content { get; }

    private IReadOnlyDictionary<AlgorithmName, ICompressionAlgorithm> Algorithms { get; }

    private CompressionLevel Level { get; }

    private ulong? MinimumSize { get; }

    #endregion

    #region Initialization

    public CompressionConcern(IHandler content, IReadOnlyDictionary<AlgorithmName, ICompressionAlgorithm> algorithms, CompressionLevel level, ulong? minimumSize)
    {
        Content = content;

        Algorithms = algorithms;
        Level = level;
        MinimumSize = minimumSize;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var acceptEncoding = request.Header.Headers.GetEntry(KnownHeaders.AcceptEncoding);

        var response = await Content.HandleAsync(request);

        if (response == null)
        {
            return null;
        }

        var content = response.Content;

        if (content != null && content.Encoding == null && response.Mode != Connection.Upgrade)
        {
            if (ShouldCompressByType(content.Type) && ShouldCompressBySize(response))
            {
                if (acceptEncoding != null)
                {
                    // todo: remove hash set

                    var supported = AcceptHeader.ParseSupported(acceptEncoding.Value.Span);

                    // todo: linq, not pre-sorted

                    foreach (var algorithm in Algorithms.Values.OrderByDescending(a => (int)a.Priority))
                    {
                        if (supported.Contains(algorithm.Name))
                        {
                            var builder = response.Rebuild();

                            builder.Content(algorithm.Compress(content, Level));

                            var vary = response.Headers.GetEntry(KnownHeaders.Vary);

                            if (vary != null)
                            {
                                var combined = new byte[vary.Value.Length + KnownHeaders.AcceptEncoding.Length + 2];

                                var span = combined.AsSpan();
                                var offset = 0;

                                vary.Value.Span.CopyTo(span);
                                offset += vary.Value.Length;

                                span[offset++] = (byte)',';
                                span[offset++] = (byte)' ';

                                KnownHeaders.AcceptEncoding.Span.CopyTo(span[offset..]);

                                builder.Header(KnownHeaders.Vary, combined);
                            }
                            else
                            {
                                builder.Header(KnownHeaders.Vary, KnownHeaders.AcceptEncoding);
                            }

                            return builder.Build();
                        }
                    }
                }
            }
        }

        return response;
    }

    private static bool ShouldCompressByType(ContentType? type)
    {
        if (type is not null)
        {
            var withoutOptions = type.Value.WithoutOptions();

            if (CompressibleTypes.Contains(withoutOptions))
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldCompressBySize(IResponse response)
    {
        var contentLength = response.Content?.Length;

        return MinimumSize is null || contentLength is null || contentLength >= MinimumSize;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
