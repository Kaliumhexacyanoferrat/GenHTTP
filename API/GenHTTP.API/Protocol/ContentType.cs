using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol
{

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
        /// Only because IE8 is really buggy.
        /// </summary>
        TextJavaScript,
        /// <summary>
        /// A JavaScript source file.
        /// </summary>
        ApplicationJavaScript,
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
        ImageIcon
    }

}
