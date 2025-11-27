using System.IO.Compression;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressionConcern : IConcern
{
    private const string AcceptEncoding = "Accept-Encoding";

    private const string Vary = "Vary";

    #region Get-/Setters

    public IHandler Content { get; }

    private IReadOnlyDictionary<string, ICompressionAlgorithm> Algorithms { get; }

    private CompressionLevel Level { get; }

    private ulong? MinimumSize { get; }

    #endregion

    #region Initialization

    public CompressionConcern(IHandler content, IReadOnlyDictionary<string, ICompressionAlgorithm> algorithms,
        CompressionLevel level, ulong? minimumSize)
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
        var response = await Content.HandleAsync(request);

        if (response?.Content != null && response.ContentEncoding == null && response.Connection != Connection.Upgrade)
        {
            if (ShouldCompressByType(request.Target.Path, response.ContentType?.KnownType) && ShouldCompressBySize(response))
            {
                if (request.Headers.TryGetValue(AcceptEncoding, out var header))
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        var supported = ParseSupported(header);

                        foreach (var algorithm in Algorithms.Values.OrderByDescending(a => (int)a.Priority))
                        {
                            if (supported.Contains(algorithm.Name))
                            {
                                response.Content = algorithm.Compress(response.Content, Level);
                                response.ContentEncoding = algorithm.Name;
                                response.ContentLength = null;

                                if (response.Headers.TryGetValue(Vary, out var existing))
                                {
                                    response.Headers[Vary] = $"{existing}, {AcceptEncoding}";
                                }
                                else
                                {
                                    response.Headers[Vary] = AcceptEncoding;
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        return response;
    }

    private static bool ShouldCompressByType(WebPath path, ContentType? type)
    {
        if (type is not null)
        {
            switch (type)
            {
                case ContentType.ApplicationJavaScript:
                case ContentType.ApplicationJson:
                case ContentType.AudioWav:
                case ContentType.TextCss:
                case ContentType.TextCsv:
                case ContentType.TextHtml:
                case ContentType.TextPlain:
                case ContentType.TextRichText:
                case ContentType.FontWoff2:
                case ContentType.FontWoff:
                case ContentType.FontTrueTypeFont:
                case ContentType.FontOpenTypeFont:
                case ContentType.FontEmbeddedOpenTypeFont:
                case ContentType.ImageScalableVectorGraphics:
                case ContentType.ImageScalableVectorGraphicsXml:
                case ContentType.ImageBmp:
                case ContentType.TextXml:
                case ContentType.TextJavaScript:
                case ContentType.ApplicationYaml:
                    {
                        return true;
                    }
            }
        }

        if (path.File is not null)
        {
            switch (Path.GetExtension(path.File))
            {
                case ".rrd": return true;
            }
        }

        return false;
    }

    private bool ShouldCompressBySize(IResponse response)
    {
        var contentLength = response.Content?.Length;

        return MinimumSize is null || contentLength is null || contentLength >= MinimumSize;
    }

    private static HashSet<string> ParseSupported(ReadOnlySpan<char> acceptHeader)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var start = 0;

        while (start <= acceptHeader.Length)
        {
            var comma = acceptHeader[start..].IndexOf(',');
            var end = comma >= 0 ? start + comma : acceptHeader.Length;

            var part = acceptHeader.Slice(start, end - start).Trim();

            if (!part.IsEmpty)
            {
                result.Add(part.ToString());
            }

            if (comma < 0)
            {
                break;
            }

            start = end + 1;
        }

        return result;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
