using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Http
{

    public static class Mapping
    {

        /// <summary>
        /// Retrieve the content type string of a content type.
        /// </summary>
        /// <param name="type">The type to convert</param>
        /// <returns>The converted string</returns>
        public static string GetContentType(ContentType type)
        {
            if (type == ContentType.TextHtml) return "text/html";
            if (type == ContentType.TextCss) return "text/css";
            if (type == ContentType.ApplicationJavaScript) return "application/javascript";
            if (type == ContentType.TextJavaScript) return "text/javascript";
            if (type == ContentType.ImageIcon) return "image/vnd.microsoft.icon";
            if (type == ContentType.ImageGif) return "image/gif";
            if (type == ContentType.ImageJpg) return "image/jpg";
            if (type == ContentType.ImagePng) return "image/png";
            if (type == ContentType.ImageBmp) return "image/bmp";
            if (type == ContentType.AudioMp4) return "audio/mp4";
            if (type == ContentType.AudioOgg) return "audio/ogg";
            if (type == ContentType.AudioMpeg) return "audio/mpeg";
            if (type == ContentType.ImageTiff) return "image/tiff";
            if (type == ContentType.TextCsv) return "text/csv";
            if (type == ContentType.TextRichText) return "text/richtext";
            if (type == ContentType.TextPlain) return "text/plain";
            if (type == ContentType.TextXml) return "text/xml";
            if (type == ContentType.VideoH264) return "video/H264";
            if (type == ContentType.VideoMp4) return "video/mp4";
            if (type == ContentType.VideoMpeg) return "video/mpeg";
            if (type == ContentType.VideoMpeg4Generic) return "video/mpeg4-generic";
            if (type == ContentType.AudioWav) return "audio/wav";
            if (type == ContentType.ApplicationOfficeDocumentWordProcessing) return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            if (type == ContentType.ApplicationOfficeDocumentPresentation) return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            if (type == ContentType.ApplicationOfficeDocumentSlideshow) return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
            if (type == ContentType.ApplicationOfficeDocumentSheet) return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return "application/force-download";
        }

        /// <summary>
        /// Try to retrieve the <see cref="ContentType" /> of a file by its extension.
        /// </summary>
        /// <param name="extension">The extension of the file (without the dot)</param>
        /// <returns>The content type to send the file with</returns>
        public static ContentType GetContentTypeByExtension(string extension)
        {
            if (extension == null) return ContentType.ApplicationForceDownload;
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
                // All other files
                default: return ContentType.ApplicationForceDownload;
            }
        }

        /// <summary>
        /// Retrieve the name of a status by its code.
        /// </summary>
        /// <param name="type">The response type to convert</param>
        /// <returns>The name of the status</returns>
        public static string GetStatusCode(ResponseType type)
        {
            if (type == ResponseType.Accepted) return "202 Accepted";
            if (type == ResponseType.BadGateway) return "502 Bad Gateway";
            if (type == ResponseType.BadRequest) return "400 Bad Request";
            if (type == ResponseType.Created) return "201 Created";
            if (type == ResponseType.Forbidden) return "403 Forbidden";
            if (type == ResponseType.InternalServerError) return "500 Internal Server Error";
            if (type == ResponseType.MovedPermanently) return "301 Moved Permanently";
            if (type == ResponseType.MovedTemporarily) return "302 Moved Temporarily";
            if (type == ResponseType.NoContent) return "204 No Content";
            if (type == ResponseType.NotFound) return "404 Not Found";
            if (type == ResponseType.NotImplemented) return "501 Not Implemented";
            if (type == ResponseType.NotModified) return "304 Not Modified";
            if (type == ResponseType.OK) return "200 OK";
            if (type == ResponseType.ServiceUnavailable) return "503 Service Unavailable";
            if (type == ResponseType.Unauthorized) return "401 Unauthorized";
            return "400 Bad Request";
        }

    }

}
