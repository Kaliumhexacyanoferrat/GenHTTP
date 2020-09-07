using System.Linq;
using System.Collections.Generic;

namespace GenHTTP.Api.Protocol
{

    #region Known Types

    /// <summary>
    /// The content type of the response.
    /// </summary>
    public enum ContentType
    {

        /// <summary>
        /// A html page.
        /// </summary>
        TextHtml,

        /// <summary>
        /// A stylesheet.
        /// </summary>
        TextCss,

        /// <summary>
        /// A JavaScript source file.
        /// </summary>
        ApplicationJavaScript,

        /// <summary>
        /// A JSON file.
        /// </summary>
        ApplicationJson,

        /// <summary>
        /// A PNG image.
        /// </summary>
        ImagePng,

        /// <summary>
        /// A BMP image.
        /// </summary>
        ImageBmp,

        /// <summary>
        /// A JPG image.
        /// </summary>
        ImageJpg,

        /// <summary>
        /// A GIF image.
        /// </summary>
        ImageGif,

        /// <summary>
        /// A download.
        /// </summary>
        ApplicationForceDownload,

        /// <summary>
        /// Anything else - data.
        /// </summary>
        ApplicationOctetStream,

        /// <summary>
        /// A MP4 audio file.
        /// </summary>
        AudioMp4,

        /// <summary>
        /// A OGG audio file.
        /// </summary>
        AudioOgg,

        /// <summary>
        /// A MPEG audio file.
        /// </summary>
        AudioMpeg,

        /// <summary>
        /// A TIFF image.
        /// </summary>
        ImageTiff,

        /// <summary>
        /// A CSV file.
        /// </summary>
        TextCsv,

        /// <summary>
        /// A RTF file.
        /// </summary>
        TextRichText,

        /// <summary>
        /// Plain text.
        /// </summary>
        TextPlain,

        /// <summary>
        /// A XML file.
        /// </summary>
        TextXml,

        /// <summary>
        /// A H.264 encoded video file.
        /// </summary>
        VideoH264,

        /// <summary>
        /// A MP4 video file.
        /// </summary>
        VideoMp4,

        /// <summary>
        /// A MPEG video file.
        /// </summary>
        VideoMpeg,

        /// <summary>
        /// A MPEG-4 video file.
        /// </summary>
        VideoMpeg4Generic,

        /// <summary>
        /// A uncompressed audio file.
        /// </summary>
        AudioWav,

        /// <summary>
        /// Word processing document (e.g. docx).
        /// </summary>
        ApplicationOfficeDocumentWordProcessing,

        /// <summary>
        /// A presentation (e.g. pptx).
        /// </summary>
        ApplicationOfficeDocumentPresentation,

        /// <summary>
        /// A slideshow (e.g. .ppsx).
        /// </summary>
        ApplicationOfficeDocumentSlideshow,

        /// <summary>
        /// A sheet (e.g. .xlsx).
        /// </summary>
        ApplicationOfficeDocumentSheet,

        /// <summary>
        /// An icon.
        /// </summary>
        ImageIcon,

        /// <summary>
        /// Microsoft, embedded otf.
        /// </summary>
        FontEmbeddedOpenTypeFont,

        /// <summary>
        /// True type font (.ttf)
        /// </summary>
        FontTrueTypeFont,

        /// <summary>
        /// Woff font (.woff)
        /// </summary>
        FontWoff,

        /// <summary>
        /// Woff 2 font (.woff2)
        /// </summary>
        FontWoff2,

        /// <summary>
        /// Open type fonf (.otf)
        /// </summary>
        FontOpenTypeFont,

        /// <summary>
        /// Scalable Vector Graphics (.svg)
        /// </summary>
        ImageScalableVectorGraphics,

        /// <summary>
        /// Scalable Vector Graphics (compressed, .svgz)
        /// </summary>
        ImageScalableVectorGraphicsCompressed,

        /// <summary>
        /// Url encoded form data.
        /// </summary>
        ApplicationWwwFormUrlEncoded

    }

    #endregion

    /// <summary>
    /// The type of content which is sent to or received from a client.
    /// </summary>
    public struct FlexibleContentType
    {

        #region Get-/Setters

        /// <summary>
        /// The known, enumerated type, if any.
        /// </summary>
        public ContentType? KnownType { get; }

        /// <summary>
        /// The raw type.
        /// </summary>
        public string RawType { get; }

        #endregion

        #region Mapping

        private static readonly Dictionary<ContentType, string> MAPPING = new Dictionary<ContentType, string>
        {
            { ContentType.TextHtml, "text/html" },
            { ContentType.TextCss, "text/css" },
            { ContentType.ApplicationJavaScript, "application/javascript" },
            { ContentType.ImageIcon, "image/x-icon" },
            { ContentType.ImageGif, "image/gif" },
            { ContentType.ImageJpg, "image/jpg" },
            { ContentType.ImagePng, "image/png" },
            { ContentType.ImageBmp, "image/bmp" },
            { ContentType.AudioMp4, "audio/mp4" },
            { ContentType.AudioOgg, "audio/ogg" },
            { ContentType.AudioMpeg, "audio/mpeg" },
            { ContentType.ImageTiff, "image/tiff" },
            { ContentType.TextCsv, "text/csv" },
            { ContentType.TextRichText, "text/richtext" },
            { ContentType.TextPlain, "text/plain" },
            { ContentType.TextXml, "text/xml" },
            { ContentType.VideoH264, "video/H264" },
            { ContentType.VideoMp4, "video/mp4" },
            { ContentType.VideoMpeg, "video/mpeg" },
            { ContentType.VideoMpeg4Generic, "video/mpeg4-generic" },
            { ContentType.AudioWav, "audio/wav" },
            { ContentType.ApplicationOfficeDocumentWordProcessing, "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ContentType.ApplicationOfficeDocumentPresentation, "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ContentType.ApplicationOfficeDocumentSlideshow, "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
            { ContentType.ApplicationOfficeDocumentSheet, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ContentType.ApplicationForceDownload, "application/force-download" },
            { ContentType.ApplicationOctetStream, "application/octet-stream" },
            { ContentType.FontEmbeddedOpenTypeFont, "font/eot" },
            { ContentType.FontOpenTypeFont, "font/otf" },
            { ContentType.FontTrueTypeFont, "font/ttf" },
            { ContentType.FontWoff, "font/woff" },
            { ContentType.FontWoff2, "font/woff2" },
            { ContentType.ImageScalableVectorGraphics, "image/svg" },
            { ContentType.ImageScalableVectorGraphicsCompressed, "image/svgz" },
            { ContentType.ApplicationJson, "application/json" },
            { ContentType.ApplicationWwwFormUrlEncoded, "application/x-www-form-urlencoded" }
        };

        private static readonly Dictionary<string, ContentType> MAPPING_REVERSE = MAPPING.ToDictionary(x => x.Value, x => x.Key);

        #endregion

        #region Initialization

        /// <summary>
        /// Create a new content type from the given string.
        /// </summary>
        /// <param name="rawType">The string representation of the content type</param>
        public FlexibleContentType(string rawType)
        {
            RawType = rawType;

            if (MAPPING_REVERSE.TryGetValue(rawType, out var knownType))
            {
                KnownType = knownType;
            }
            else
            {
                KnownType = null;
            }
        }

        /// <summary>
        /// Create a new content type from the given known type.
        /// </summary>
        /// <param name="type">The known type</param>
        public FlexibleContentType(ContentType type)
        {
            KnownType = type;
            RawType = MAPPING[type];
        }

        #endregion

        #region Convenience

        public static bool operator ==(FlexibleContentType type, ContentType knownType) => type.KnownType == knownType;

        public static bool operator !=(FlexibleContentType type, ContentType knownType) => type.KnownType != knownType;

        public static bool operator ==(FlexibleContentType type, string rawType) => type.RawType == rawType;

        public static bool operator !=(FlexibleContentType type, string rawType) => type.RawType != rawType;

        public override bool Equals(object obj) => obj is FlexibleContentType type && RawType == type.RawType;

        public override int GetHashCode() => RawType.GetHashCode();

        #endregion

    }

}
