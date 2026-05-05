using System.Text;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// The type of request or response.
/// </summary>
public readonly struct ContentType
{

    private readonly ReadOnlyMemory<byte> _value;

    public ContentType(string type) : this(Encoding.ASCII.GetBytes(type))
    {

    }

    public ContentType(ReadOnlyMemory<byte> value)
    {
        _value = value;
    }

    public ReadOnlyMemory<byte> Value => _value;

    #region Known Types

    /// <summary>A html page.</summary>
    public static readonly ContentType TextHtml = new("text/html"u8.ToArray());

    /// <summary>A stylesheet.</summary>
    public static readonly ContentType TextCss = new("text/css"u8.ToArray());

    /// <summary>A human readable YAML file.</summary>
    public static readonly ContentType TextYaml = new("text/yaml"u8.ToArray());

    /// <summary>A JavaScript source file.</summary>
    public static readonly ContentType ApplicationJavaScript = new("application/javascript"u8.ToArray());

    /// <summary>A JSON file.</summary>
    public static readonly ContentType ApplicationJson = new("application/json"u8.ToArray());

    /// <summary>A YAML file.</summary>
    public static readonly ContentType ApplicationYaml = new("application/yaml"u8.ToArray());

    /// <summary>A PNG image.</summary>
    public static readonly ContentType ImagePng = new("image/png"u8.ToArray());

    /// <summary>A BMP image.</summary>
    public static readonly ContentType ImageBmp = new("image/bmp"u8.ToArray());

    /// <summary>A JPG image.</summary>
    public static readonly ContentType ImageJpg = new("image/jpg"u8.ToArray());

    /// <summary>A GIF image.</summary>
    public static readonly ContentType ImageGif = new("image/gif"u8.ToArray());

    /// <summary>A download.</summary>
    public static readonly ContentType ApplicationForceDownload = new("application/force-download"u8.ToArray());

    /// <summary>Anything else - data.</summary>
    public static readonly ContentType ApplicationOctetStream = new("application/octet-stream"u8.ToArray());

    /// <summary>A MP4 audio file.</summary>
    public static readonly ContentType AudioMp4 = new("audio/mp4"u8.ToArray());

    /// <summary>A OGG audio file.</summary>
    public static readonly ContentType AudioOgg = new("audio/ogg"u8.ToArray());

    /// <summary>A MPEG audio file.</summary>
    public static readonly ContentType AudioMpeg = new("audio/mpeg"u8.ToArray());

    /// <summary>A TIFF image.</summary>
    public static readonly ContentType ImageTiff = new("image/tiff"u8.ToArray());

    /// <summary>A CSV file.</summary>
    public static readonly ContentType TextCsv = new("text/csv"u8.ToArray());

    /// <summary>A RTF file.</summary>
    public static readonly ContentType TextRichText = new("text/richtext"u8.ToArray());

    /// <summary>Plain text.</summary>
    public static readonly ContentType TextPlain = new("text/plain"u8.ToArray());

    /// <summary>A XML file.</summary>
    public static readonly ContentType TextXml = new("text/xml"u8.ToArray());

    /// <summary>A JavaScript file.</summary>
    public static readonly ContentType TextJavaScript = new("text/javascript"u8.ToArray());

    /// <summary>A SSE stream.</summary>
    public static readonly ContentType TextEventStream = new("text/event-stream"u8.ToArray());

    /// <summary>A uncompressed audio file.</summary>
    public static readonly ContentType AudioWav = new("audio/wav"u8.ToArray());

    /// <summary>Word processing document (e.g. docx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentWordProcessing = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document"u8.ToArray());

    /// <summary>A presentation (e.g. pptx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentPresentation = new("application/vnd.openxmlformats-officedocument.presentationml.presentation"u8.ToArray());

    /// <summary>A slideshow (e.g. .ppsx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentSlideshow = new("application/vnd.openxmlformats-officedocument.presentationml.slideshow"u8.ToArray());

    /// <summary>A sheet (e.g. .xlsx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentSheet = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"u8.ToArray());

    /// <summary>An icon.</summary>
    public static readonly ContentType ImageIcon = new("image/x-icon"u8.ToArray());

    /// <summary>Microsoft embedded OTF.</summary>
    public static readonly ContentType FontEmbeddedOpenTypeFont = new("font/eot"u8.ToArray());

    /// <summary>True type font.</summary>
    public static readonly ContentType FontTrueTypeFont = new("font/ttf"u8.ToArray());

    /// <summary>Woff font.</summary>
    public static readonly ContentType FontWoff = new("font/woff"u8.ToArray());

    /// <summary>Woff2 font.</summary>
    public static readonly ContentType FontWoff2 = new("font/woff2"u8.ToArray());

    /// <summary>Open type font.</summary>
    public static readonly ContentType FontOpenTypeFont = new("font/otf"u8.ToArray());

    /// <summary>SVG.</summary>
    public static readonly ContentType ImageScalableVectorGraphics = new("image/svg"u8.ToArray());

    /// <summary>SVG XML.</summary>
    public static readonly ContentType ImageScalableVectorGraphicsXml = new("image/svg+xml"u8.ToArray());

    /// <summary>Compressed SVG.</summary>
    public static readonly ContentType ImageScalableVectorGraphicsCompressed = new("image/svgz"u8.ToArray());

    /// <summary>Form encoded.</summary>
    public static readonly ContentType ApplicationWwwFormUrlEncoded = new("application/x-www-form-urlencoded"u8.ToArray());

    /// <summary>Protobuf.</summary>
    public static readonly ContentType ApplicationProtobuf = new("application/protobuf"u8.ToArray());

    /// <summary>PDF.</summary>
    public static readonly ContentType ApplicationPdf = new("application/pdf"u8.ToArray());

    /// <summary>3GPP video.</summary>
    public static readonly ContentType Video3Gpp = new("video/3gpp"u8.ToArray());

    /// <summary>3GPP2 video.</summary>
    public static readonly ContentType Video3Gpp2 = new("video/3gpp2"u8.ToArray());

    /// <summary>AV1 video.</summary>
    public static readonly ContentType VideoAV1 = new("video/av1"u8.ToArray());

    /// <summary>AVC video.</summary>
    public static readonly ContentType VideoAvc = new("video/av"u8.ToArray());

    /// <summary>DV video.</summary>
    public static readonly ContentType VideoDV = new("video/dv"u8.ToArray());

    /// <summary>H261 video.</summary>
    public static readonly ContentType VideoH261 = new("video/H261"u8.ToArray());

    /// <summary>H263 video.</summary>
    public static readonly ContentType VideoH263 = new("video/H263"u8.ToArray());

    /// <summary>H264 video.</summary>
    public static readonly ContentType VideoH264 = new("video/H264"u8.ToArray());

    /// <summary>H265 video.</summary>
    public static readonly ContentType VideoH265 = new("video/H265"u8.ToArray());

    /// <summary>H266 video.</summary>
    public static readonly ContentType VideoH266 = new("video/H266"u8.ToArray());

    /// <summary>Matroska.</summary>
    public static readonly ContentType VideoMatroska = new("video/matroska"u8.ToArray());

    /// <summary>Matroska 3D.</summary>
    public static readonly ContentType VideoMatroska3D = new("video/matroska-3d"u8.ToArray());

    /// <summary>MJ2.</summary>
    public static readonly ContentType VideoMJ2 = new("video/mj2"u8.ToArray());

    /// <summary>MP4 video.</summary>
    public static readonly ContentType VideoMP4 = new("video/mp4"u8.ToArray());

    /// <summary>MPEG video.</summary>
    public static readonly ContentType VideoMpeg = new("video/mpeg"u8.ToArray());

    /// <summary>MPEG4 generic.</summary>
    public static readonly ContentType VideoMpeg4Generic = new("video/mpeg4-generic"u8.ToArray());

    /// <summary>MPV.</summary>
    public static readonly ContentType VideoMpv = new("video/MPV"u8.ToArray());

    /// <summary>Quicktime.</summary>
    public static readonly ContentType VideoQuicktime = new("video/quicktime"u8.ToArray());

    /// <summary>Raw video.</summary>
    public static readonly ContentType VideoRaw = new("video/raw"u8.ToArray());

    /// <summary>VC1.</summary>
    public static readonly ContentType VideoVC1 = new("video/vc1"u8.ToArray());

    /// <summary>VC2.</summary>
    public static readonly ContentType VideoVC2 = new("video/vc2"u8.ToArray());

    /// <summary>VP8.</summary>
    public static readonly ContentType VideoVP8 = new("video/VP8"u8.ToArray());

    /// <summary>VP9.</summary>
    public static readonly ContentType VideoVP9 = new("video/VP9"u8.ToArray());

    /// <summary>WebM.</summary>
    public static readonly ContentType VideoWebM = new("video/webm"u8.ToArray());

    #endregion

}

