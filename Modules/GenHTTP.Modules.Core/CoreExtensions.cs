using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core
{

    public static class CoreExtensions
    {

        public static ContentType? GuessContentType(this string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if ((extension != null) && (extension.Length > 1))
            {
                extension = extension.Substring(1);

                switch (extension.ToLower())
                {
                    // CSS file
                    case "css": return ContentType.TextCss;
                    // HTML files
                    case "html":
                    case "htm": return ContentType.TextHtml;
                    // Text files
                    case "sql":
                    case "txt":
                    case "pl":
                    case "cs":
                    case "php":
                    case "c":
                    case "h":
                    case "cpp":
                    case "sh":
                    case "bat":
                    case "cmd":
                    case "conf":
                    case "ini":
                    case "inf": return ContentType.TextPlain;
                    // Fonts
                    case "eot": return ContentType.FontEmbeddedOpenTypeFont;
                    case "ttf": return ContentType.FontTrueTypeFont;
                    case "otf": return ContentType.FontOpenTypeFont;
                    case "woff": return ContentType.FontWoff;
                    case "woff2": return ContentType.FontWoff2;
                    // JavaScript
                    case "js": return ContentType.ApplicationJavaScript;
                    // Images
                    case "ico": return ContentType.ImageIcon;
                    case "gif": return ContentType.ImageGif;
                    case "jpeg":
                    case "jpg": return ContentType.ImageJpg;
                    case "png": return ContentType.ImagePng;
                    case "bmp": return ContentType.ImageBmp;
                    case "tiff": return ContentType.ImageTiff;
                    case "svg": return ContentType.ImageScalableVectorGraphics;
                    case "svgz": return ContentType.ImageScalableVectorGraphicsCompressed;
                    // Audio
                    case "ogg": return ContentType.AudioOgg;
                    case "mp3": return ContentType.AudioMpeg;
                    case "wav": return ContentType.AudioWav;
                    // Video files
                    case "mpg":
                    case "mpeg":
                    case "avi": return ContentType.VideoMpeg;
                    // Object models
                    case "xml": return ContentType.TextXml;
                    // Documents
                    case "csv": return ContentType.TextCsv;
                    case "rtf": return ContentType.TextRichText;
                    case "docx": return ContentType.ApplicationOfficeDocumentWordProcessing;
                    case "pptx": return ContentType.ApplicationOfficeDocumentPresentation;
                    case "ppsx": return ContentType.ApplicationOfficeDocumentSlideshow;
                    case "xlsx": return ContentType.ApplicationOfficeDocumentSheet;
                }
            }

            return null;
        }

        public static IResponseBuilder Respond(this IRequest request, ResponseType type)
        {
            var provider = request.Routing?.Router.GetErrorHandler(request, type) ?? throw new InvalidOperationException("Routing context is missing");
            return provider.Handle(request).Type(type);
        }

        public static bool HasType(this IRequest request, params RequestType[] types)
        {
            foreach (var type in types)
            {
                if (type == request.Type)
                {
                    return true;
                }
            }

            return false;
        }


    }

}
