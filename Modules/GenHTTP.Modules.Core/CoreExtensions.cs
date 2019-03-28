using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core
{

    public static class CoreExtensions
    {

        public static ContentType GuessContentType(this string fileName)
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
                    case "ico": return ContentType.ImageIcon;
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
                    // JavaScript
                    case "js": return ContentType.ApplicationJavaScript;
                    // GIF
                    case "gif": return ContentType.ImageGif;
                    // JPG
                    case "jpeg":
                    case "jpg": return ContentType.ImageJpg;
                    // PNG
                    case "png": return ContentType.ImagePng;
                    // BMP
                    case "bmp": return ContentType.ImageBmp;
                    // OGG
                    case "ogg": return ContentType.AudioOgg;
                    // MP3
                    case "mp3": return ContentType.AudioMpeg;
                    // WAV
                    case "wav": return ContentType.AudioWav;
                    // TIFF
                    case "tiff": return ContentType.ImageTiff;
                    // CSV
                    case "csv": return ContentType.TextCsv;
                    // RTF
                    case "rtf": return ContentType.TextRichText;
                    // XML
                    case "xml": return ContentType.TextXml;
                    // Video files
                    case "mpg":
                    case "mpeg":
                    case "avi": return ContentType.VideoMpeg;
                    // Word processing
                    case "docx": return ContentType.ApplicationOfficeDocumentWordProcessing;
                    // Presentation
                    case "pptx": return ContentType.ApplicationOfficeDocumentPresentation;
                    // Slideshow
                    case "ppsx": return ContentType.ApplicationOfficeDocumentSlideshow;
                    // Sheet
                    case "xlsx": return ContentType.ApplicationOfficeDocumentSheet;
                }
            }

            // All other files
            return ContentType.ApplicationForceDownload;
        }

    }

}
