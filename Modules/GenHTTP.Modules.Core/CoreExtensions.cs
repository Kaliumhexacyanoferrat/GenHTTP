using System;
using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core
{

    public static class CoreExtensions
    {

        #region Mappings

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

        #endregion

        #region Functionality

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

        public static IResponseBuilder Respond(this IRequest request, ResponseStatus status, Exception? cause = null)
        {
            var provider = request.Routing?.Router.GetErrorHandler(request, status, cause) ?? throw new InvalidOperationException("Routing context is missing");
            return provider.Handle(request).Status(status);
        }

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

        public static string GetResourceAsString(this IResourceProvider resourceProvider)
        {
            using var stream = resourceProvider.GetResource();
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
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

    }

}
