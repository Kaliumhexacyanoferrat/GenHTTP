using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core
{

    public static class CoreExtensions
    {

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

            if (host != null)
            {
                var pos = host.IndexOf(':');

                if (pos > 0)
                {
                    return host.Substring(0, pos);
                }
                else
                {
                    return host;
                }
            }

            return null;
        }

        #endregion

        #region Response builder

        /// <summary>
        /// Specifies the content type of this response.
        /// </summary>
        /// <param name="contentType">The content type of this response</param>
        public static IResponseBuilder Type(this IResponseBuilder builder, ContentType contentType) => builder.Type(new FlexibleContentType(contentType));

        /// <summary>
        /// Specifies the content type of this response.
        /// </summary>
        /// <param name="contentType">The content type of this response</param>
        public static IResponseBuilder Type(this IResponseBuilder builder, string contentType) => builder.Type(new FlexibleContentType(contentType));

        #endregion

        #region Content types

        private static readonly Dictionary<string, ContentType> CONTENT_TYPES = new Dictionary<string, ContentType>() {
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
            // Images
            { "ico", ContentType.ImageIcon },
            { "gif", ContentType.ImageGif },
            { "jpeg", ContentType.ImageJpg },
            { "jpg", ContentType.ImageJpg },
            { "png", ContentType.ImagePng },
            { "bmp", ContentType.ImageBmp },
            { "tiff", ContentType.ImageTiff },
            { "svg", ContentType.ImageScalableVectorGraphics },
            { "svgz", ContentType.ImageScalableVectorGraphicsCompressed },
            // Audio
            { "ogg", ContentType.AudioOgg },
            { "mp3", ContentType.AudioMpeg },
            { "wav", ContentType.AudioWav },
            // Video
            { "mpg", ContentType.VideoMpeg },
            { "mpeg", ContentType.VideoMpeg },
            { "avi", ContentType.VideoMpeg },
            // Documents
            { "csv", ContentType.TextCsv },
            { "rtf", ContentType.TextRichText },
            { "docx", ContentType.ApplicationOfficeDocumentWordProcessing },
            { "pptx", ContentType.ApplicationOfficeDocumentPresentation },
            { "ppsx", ContentType.ApplicationOfficeDocumentSlideshow },
            { "xslx", ContentType.ApplicationOfficeDocumentSheet },
            // Object models
            { "xml", ContentType.TextXml }
        };

        public static ContentType? GuessContentType(this string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if ((extension != null) && (extension.Length > 1))
            {
                extension = extension.Substring(1).ToLower();

                if (CONTENT_TYPES.ContainsKey(extension))
                {
                    return CONTENT_TYPES[extension];
                }
            }

            return null;
        }

        #endregion

        #region Resource provider

        public static string GetResourceAsString(this IResourceProvider resourceProvider)
        {
            using var stream = resourceProvider.GetResource();

            return new StreamReader(stream).ReadToEnd();
        }

        #endregion

    }

}
