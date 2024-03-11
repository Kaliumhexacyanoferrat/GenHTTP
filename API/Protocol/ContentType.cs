using System;
using System.Collections.Generic;
using System.Linq;

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
        /// A JavaScript file.
        /// </summary>
        TextJavaScript,

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
        /// Scalable Vector Graphics (.svg)
        /// </summary>
        ImageScalableVectorGraphicsXml,

        /// <summary>
        /// Scalable Vector Graphics (compressed, .svgz)
        /// </summary>
        ImageScalableVectorGraphicsCompressed,

        /// <summary>
        /// Url encoded form data.
        /// </summary>
        ApplicationWwwFormUrlEncoded,

        /// <summary>
        /// A Protobuf message.
        /// </summary>
        ApplicationProtobuf,

        /// <summary>
        /// 3GPP video file container (.3gp).
        /// </summary>
        Video3Gpp,

        /// <summary>
        /// 3GPP2 video files (.3g2).
        /// </summary>
        Video3Gpp2,

        /// <summary>
        /// AV1 video file (.av1).
        /// </summary>
        VideoAV1,

        /// <summary>
        /// A MPEG4 Part 10 (H.264) video file (.avc).
        /// </summary>
        VideoAvc,

        /// <summary>
        /// Digital video file (.dv).
        /// </summary>
        VideoDV,

        /// <summary>
        /// A H.261 video file.
        /// </summary>
        VideoH261,

        /// <summary>
        /// A H.263 video file.
        /// </summary>
        VideoH263,

        /// <summary>
        /// A H.264 encoded video file.
        /// </summary>
        VideoH264,

        /// <summary>
        /// A H.265 video file.
        /// </summary>
        VideoH265,

        /// <summary>
        /// A H.266 video file.
        /// </summary>
        VideoH266,

        /// <summary>
        /// A Matroska video file (.mkv).
        /// </summary>
        VideoMatroska,

        /// <summary>
        /// A 3D Matroska video file (.mk3d).
        /// </summary>
        VideoMatroska3D,

        /// <summary>
        /// A Motion JPEG 2000 video file (.mj2).
        /// </summary>
        VideoMJ2,

        /// <summary>
        /// A MP4 video file (.mp4).
        /// </summary>
        VideoMP4,

        /// <summary>
        /// A MPEG video file.
        /// </summary>
        VideoMpeg,

        /// <summary>
        /// A MPEG-4 video file.
        /// </summary>
        VideoMpeg4Generic,

        /// <summary>
        /// A MPEG-2 elementary stream video (.mpv).
        /// </summary>
        VideoMpv,

        /// <summary>
        /// An Apple quick time video file (.mov or .hdmov).
        /// </summary>
        VideoQuicktime,

        /// <summary>
        /// A raw video file.
        /// </summary>
        VideoRaw,

        /// <summary>
        /// A SMPTE 421M video file (.vc1).
        /// </summary>
        VideoVC1,

        /// <summary>
        /// A SMPTE VC-2 video file.
        /// </summary>
        VideoVC2,

        /// <summary>
        /// A VP8 encoded video file (.webm).
        /// </summary>
        VideoVP8,

        /// <summary>
        /// A VP9 encoded video file (.webm).
        /// </summary>
        VideoVP9,

        /// <summary>
        /// A WebM video file (.webm).
        /// </summary>
        VideoWebM

    }

    #endregion

    /// <summary>
    /// The type of content which is sent to or received from a client.
    /// </summary>
    public class FlexibleContentType
    {
        private static readonly Dictionary<string, FlexibleContentType> _RawCache = new(StringComparer.InvariantCultureIgnoreCase);

        private static readonly Dictionary<ContentType, FlexibleContentType> _KnownCache = new();

        #region Get-/Setters

        /// <summary>
        /// The known, enumerated type, if any.
        /// </summary>
        public ContentType? KnownType { get; }

        /// <summary>
        /// The raw type.
        /// </summary>
        public string RawType { get; }

        /// <summary>
        /// The charset of the content, if any.
        /// </summary>
        public string? Charset { get; }

        #endregion

        #region Mapping

        private static readonly Dictionary<ContentType, string> MAPPING = new()
        {
            { ContentType.AudioMp4, "audio/mp4" },
            { ContentType.AudioOgg, "audio/ogg" },
            { ContentType.AudioMpeg, "audio/mpeg" },
            { ContentType.AudioWav, "audio/wav" },
            { ContentType.ApplicationJavaScript, "application/javascript" },
            { ContentType.ApplicationOfficeDocumentWordProcessing, "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ContentType.ApplicationOfficeDocumentPresentation, "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ContentType.ApplicationOfficeDocumentSlideshow, "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
            { ContentType.ApplicationOfficeDocumentSheet, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ContentType.ApplicationForceDownload, "application/force-download" },
            { ContentType.ApplicationOctetStream, "application/octet-stream" },
            { ContentType.ApplicationJson, "application/json" },
            { ContentType.ApplicationWwwFormUrlEncoded, "application/x-www-form-urlencoded" },
            { ContentType.ApplicationProtobuf, "application/protobuf" },
            { ContentType.FontEmbeddedOpenTypeFont, "font/eot" },
            { ContentType.FontOpenTypeFont, "font/otf" },
            { ContentType.FontTrueTypeFont, "font/ttf" },
            { ContentType.FontWoff, "font/woff" },
            { ContentType.FontWoff2, "font/woff2" },
            { ContentType.ImageIcon, "image/x-icon" },
            { ContentType.ImageGif, "image/gif" },
            { ContentType.ImageJpg, "image/jpg" },
            { ContentType.ImagePng, "image/png" },
            { ContentType.ImageTiff, "image/tiff" },
            { ContentType.ImageBmp, "image/bmp" },
            { ContentType.ImageScalableVectorGraphics, "image/svg" },
            { ContentType.ImageScalableVectorGraphicsXml, "image/svg+xml" },
            { ContentType.ImageScalableVectorGraphicsCompressed, "image/svgz" },
            { ContentType.TextHtml, "text/html" },
            { ContentType.TextCss, "text/css" },
            { ContentType.TextCsv, "text/csv" },
            { ContentType.TextRichText, "text/richtext" },
            { ContentType.TextPlain, "text/plain" },
            { ContentType.TextJavaScript, "text/javascript" },
            { ContentType.TextXml, "text/xml" },
            { ContentType.Video3Gpp, "video/3gpp" },
            { ContentType.Video3Gpp2, "video/3gpp2" },
            { ContentType.VideoAV1, "video/av1" },
            { ContentType.VideoAvc, "video/av" },
            { ContentType.VideoDV, "video/dv" },
            { ContentType.VideoH261, "video/H261" },
            { ContentType.VideoH263, "video/H263" },
            { ContentType.VideoH264, "video/H264" },
            { ContentType.VideoH265, "video/H265" },
            { ContentType.VideoH266, "video/H266" },
            { ContentType.VideoMatroska, "video/matroska" },
            { ContentType.VideoMatroska3D, "video/matroska-3d" },
            { ContentType.VideoMJ2, "video/mj2" },
            { ContentType.VideoMP4, "video/mp4" },
            { ContentType.VideoMpeg, "video/mpeg" },
            { ContentType.VideoMpeg4Generic, "video/mpeg4-generic" },
            { ContentType.VideoMpv, "video/MPV" },
            { ContentType.VideoQuicktime, "video/quicktime" },
            { ContentType.VideoRaw, "video/raw" },
            { ContentType.VideoVC1, "video/vc1" },
            { ContentType.VideoVC2, "video/vc2" },
            { ContentType.VideoVP8, "video/VP8" },
            { ContentType.VideoVP9, "video/VP9" },
            { ContentType.VideoWebM, "video/webm" }
        };

        private static readonly Dictionary<string, ContentType> MAPPING_REVERSE = MAPPING.ToDictionary(x => x.Value, x => x.Key);

        #endregion

        #region Initialization

        /// <summary>
        /// Create a new content type from the given string.
        /// </summary>
        /// <param name="rawType">The string representation of the content type</param>
        /// <param name="charset">The charset of the content, if known</param>
        public FlexibleContentType(string rawType, string? charset = null)
        {
            RawType = rawType;
            Charset = charset;

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
        /// <param name="charset">The charset of the content, if known</param>
        public FlexibleContentType(ContentType type, string? charset = null)
        {
            KnownType = type;
            RawType = MAPPING[type];

            Charset = charset;
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Fetches a cached instance for the given content type.
        /// </summary>
        /// <param name="rawType">The raw string to be resolved</param>
        /// <returns>The content type instance to be used</returns>
        public static FlexibleContentType Get(string rawType, string? charset = null)
        {
            if (charset is not null)
            {
                return new(rawType, charset);
            }

            if (_RawCache.TryGetValue(rawType, out var found))
            {
                return found;
            }

            var type = new FlexibleContentType(rawType);

            _RawCache[rawType] = type;

            return type;
        }

        /// <summary>
        /// Fetches a cached instance for the given content type.
        /// </summary>
        /// <param name="knownType">The known type to be resolved</param>
        /// <returns>The content type instance to be used</returns>
        public static FlexibleContentType Get(ContentType knownType, string? charset = null)
        {
            if (charset is not null)
            {
                return new(knownType, charset);
            }

            if (_KnownCache.TryGetValue(knownType, out var found))
            {
                return found;
            }

            var type = new FlexibleContentType(knownType);

            _KnownCache[knownType] = type;

            return type;
        }

        /// <summary>
        /// Parses the given header value into a content type structure.
        /// </summary>
        /// <param name="header">The header to be parsed</param>
        /// <returns>The parsed content type</returns>
        public static FlexibleContentType Parse(string header)
        {
            var span = header.AsSpan();

            var index = span.IndexOf(';');

            if (index > 0)
            {
                var contentType = span[..index].Trim().ToString();

                var charsetIndex = span.IndexOf('=');

                if (charsetIndex > 0)
                {
                    return Get(contentType, span[(charsetIndex + 1)..].Trim().ToString());
                }
                else
                {
                    return Get(contentType);
                }
            }
            else
            {
                return Get(header);
            }
        }

        #endregion

        #region Convenience

        public static bool operator ==(FlexibleContentType type, ContentType knownType) => type.KnownType == knownType;

        public static bool operator !=(FlexibleContentType type, ContentType knownType) => type.KnownType != knownType;

        public static bool operator ==(FlexibleContentType type, string rawType) => type.RawType == rawType;

        public static bool operator !=(FlexibleContentType type, string rawType) => type.RawType != rawType;

        public override bool Equals(object? obj) => obj is FlexibleContentType type && RawType == type.RawType;

        public override int GetHashCode() => RawType.GetHashCode();

        #endregion

    }

}
