using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class ContentTypeGuessingExtensions
{

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

    #region Mapping

    private static readonly Dictionary<string, ContentType> ContentTypes = new()
    {
        // CSS
        {
            "css", ContentType.TextCss
        },
        // HTML
        {
            "html", ContentType.TextHtml
        },
        {
            "htm", ContentType.TextHtml
        },
        // Documents
        {
            "txt", ContentType.TextPlain
        },
        {
            "conf", ContentType.TextPlain
        },
        {
            "config", ContentType.TextPlain
        },
        // Fonts
        {
            "eot", ContentType.FontEmbeddedOpenTypeFont
        },
        {
            "ttf", ContentType.FontTrueTypeFont
        },
        {
            "otf", ContentType.FontOpenTypeFont
        },
        {
            "woff", ContentType.FontWoff
        },
        {
            "woff2", ContentType.FontWoff2
        },
        // Scripts
        {
            "js", ContentType.ApplicationJavaScript
        },
        {
            "mjs", ContentType.ApplicationJavaScript
        },
        // Images
        {
            "ico", ContentType.ImageIcon
        },
        {
            "gif", ContentType.ImageGif
        },
        {
            "jpeg", ContentType.ImageJpg
        },
        {
            "jpg", ContentType.ImageJpg
        },
        {
            "png", ContentType.ImagePng
        },
        {
            "bmp", ContentType.ImageBmp
        },
        {
            "tiff", ContentType.ImageTiff
        },
        {
            "svg", ContentType.ImageScalableVectorGraphicsXml
        },
        {
            "svgz", ContentType.ImageScalableVectorGraphicsCompressed
        },
        // Audio
        {
            "ogg", ContentType.AudioOgg
        },
        {
            "mp3", ContentType.AudioMpeg
        },
        {
            "wav", ContentType.AudioWav
        },
        // Video
        {
            "avi", ContentType.VideoMpeg
        },
        {
            "3gp", ContentType.Video3Gpp
        },
        {
            "3g2", ContentType.Video3Gpp2
        },
        {
            "av1", ContentType.VideoAV1
        },
        {
            "avc", ContentType.VideoAvc
        },
        {
            "dv", ContentType.VideoDV
        },
        {
            "mkv", ContentType.VideoMatroska
        },
        {
            "mk3d", ContentType.VideoMatroska3D
        },
        {
            "mj2", ContentType.VideoMJ2
        },
        {
            "mpg", ContentType.VideoMpeg
        },
        {
            "mp4", ContentType.VideoMP4
        },
        {
            "mpeg", ContentType.VideoMpeg
        },
        {
            "mpv", ContentType.VideoMpv
        },
        {
            "mov", ContentType.VideoQuicktime
        },
        {
            "hdmov", ContentType.VideoQuicktime
        },
        {
            "vc1", ContentType.VideoVC1
        },
        {
            "vc2", ContentType.VideoVC2
        },
        {
            "webm", ContentType.VideoWebM
        },
        // Documents
        {
            "csv", ContentType.TextCsv
        },
        {
            "rtf", ContentType.TextRichText
        },
        {
            "docx", ContentType.ApplicationOfficeDocumentWordProcessing
        },
        {
            "pptx", ContentType.ApplicationOfficeDocumentPresentation
        },
        {
            "ppsx", ContentType.ApplicationOfficeDocumentSlideshow
        },
        {
            "xslx", ContentType.ApplicationOfficeDocumentSheet
        },
        // Object models
        {
            "json", ContentType.ApplicationJson
        },
        {
            "xml", ContentType.TextXml
        },
        {
            "yml", ContentType.TextYaml
        },
        {
            "yaml", ContentType.TextYaml
        }
    };

    #endregion

}
