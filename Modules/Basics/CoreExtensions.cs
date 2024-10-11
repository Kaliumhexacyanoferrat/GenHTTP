using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Basics;

public static class CoreExtensions
{

    #region Resource provider

    public static async ValueTask<string> GetResourceAsStringAsync(this IResource resourceProvider)
    {
        await using var stream = await resourceProvider.GetContentAsync();

        return await new StreamReader(stream).ReadToEndAsync();
    }

    #endregion

    #region Request

    public static bool HasType(this IRequest request, params RequestMethod[] methods)
    {
        foreach (var method in methods)
        {
            if (request.Method == method)
            {
                return true;
            }
        }

        return false;
    }

    public static string? HostWithoutPort(this IRequest request)
    {
        var host = request.Host;

        if (host is not null)
        {
            var pos = host.IndexOf(':');

            return pos > 0 ? host[..pos] : host;
        }

        return null;
    }

    #endregion

    #region Response builder

    /// <summary>
    /// Specifies the content type of this response.
    /// </summary>
    /// <param name="contentType">The content type of this response</param>
    public static IResponseBuilder Type(this IResponseBuilder builder, ContentType contentType) => builder.Type(FlexibleContentType.Get(contentType));

    /// <summary>
    /// Specifies the content type of this response.
    /// </summary>
    /// <param name="contentType">The content type of this response</param>
    public static IResponseBuilder Type(this IResponseBuilder builder, string contentType) => builder.Type(FlexibleContentType.Parse(contentType));

    #endregion

    #region Content types

    private static readonly Dictionary<string, ContentType> ContentTypes = new()
    {
        // CSS
        { "css", ContentType.TextCss },
        // HTML
        { "html", ContentType.TextHtml },
        { "htm", ContentType.TextHtml },
        // Text files
        { "txt", ContentType.TextPlain },
        { "conf", ContentType.TextPlain },
        { "config", ContentType.TextPlain },
        // Fonts
        { "eot", ContentType.FontEmbeddedOpenTypeFont },
        { "ttf", ContentType.FontTrueTypeFont },
        { "otf", ContentType.FontOpenTypeFont },
        { "woff", ContentType.FontWoff },
        { "woff2", ContentType.FontWoff2 },
        // Scripts
        { "js", ContentType.ApplicationJavaScript },
        { "mjs", ContentType.ApplicationJavaScript },
        // Images
        { "ico", ContentType.ImageIcon },
        { "gif", ContentType.ImageGif },
        { "jpeg", ContentType.ImageJpg },
        { "jpg", ContentType.ImageJpg },
        { "png", ContentType.ImagePng },
        { "bmp", ContentType.ImageBmp },
        { "tiff", ContentType.ImageTiff },
        { "svg", ContentType.ImageScalableVectorGraphicsXml },
        { "svgz", ContentType.ImageScalableVectorGraphicsCompressed },
        // Audio
        { "ogg", ContentType.AudioOgg },
        { "mp3", ContentType.AudioMpeg },
        { "wav", ContentType.AudioWav },
        // Video
        { "avi", ContentType.VideoMpeg },
        { "3gp", ContentType.Video3Gpp },
        { "3g2", ContentType.Video3Gpp2 },
        { "av1", ContentType.VideoAV1 },
        { "avc", ContentType.VideoAvc },
        { "dv", ContentType.VideoDV },
        { "mkv", ContentType.VideoMatroska },
        { "mk3d", ContentType.VideoMatroska3D },
        { "mj2", ContentType.VideoMJ2 },
        { "mpg", ContentType.VideoMpeg },
        { "mp4", ContentType.VideoMP4 },
        { "mpeg", ContentType.VideoMpeg },
        { "mpv", ContentType.VideoMpv },
        { "mov", ContentType.VideoQuicktime },
        { "hdmov", ContentType.VideoQuicktime },
        { "vc1", ContentType.VideoVC1 },
        { "vc2", ContentType.VideoVC2 },
        { "webm", ContentType.VideoWebM },
        // Documents
        { "csv", ContentType.TextCsv },
        { "rtf", ContentType.TextRichText },
        { "docx", ContentType.ApplicationOfficeDocumentWordProcessing },
        { "pptx", ContentType.ApplicationOfficeDocumentPresentation },
        { "ppsx", ContentType.ApplicationOfficeDocumentSlideshow },
        { "xslx", ContentType.ApplicationOfficeDocumentSheet },
        // Object models
        { "json", ContentType.ApplicationJson },
        { "xml", ContentType.TextXml }
    };

    public static ContentType? GuessContentType(this string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (extension.Length > 1)
        {
            extension = extension[1..].ToLower();

            if (ContentTypes.TryGetValue(extension, out var value))
            {
                return value;
            }
        }

        return null;
    }

    #endregion

}
