namespace GenHTTP.Api.Protocol;

/// <summary>Represents an HTTP Content-Type.</summary>
[MemoryView]
public readonly partial struct ContentType
{

    #region Known Types

    /// <summary>A html page.</summary>
    public static readonly ContentType TextHtml = new("text/html");

    /// <summary>A stylesheet.</summary>
    public static readonly ContentType TextCss = new("text/css");

    /// <summary>A human-readable YAML file.</summary>
    public static readonly ContentType TextYaml = new("text/yaml");

    /// <summary>A JavaScript source file.</summary>
    public static readonly ContentType ApplicationJavaScript = new("application/javascript");

    /// <summary>A JSON file.</summary>
    public static readonly ContentType ApplicationJson = new("application/json");

    /// <summary>A YAML file.</summary>
    public static readonly ContentType ApplicationYaml = new("application/yaml");

    /// <summary>A PNG image.</summary>
    public static readonly ContentType ImagePng = new("image/png");

    /// <summary>A BMP image.</summary>
    public static readonly ContentType ImageBmp = new("image/bmp");

    /// <summary>A JPG image.</summary>
    public static readonly ContentType ImageJpg = new("image/jpg");

    /// <summary>A GIF image.</summary>
    public static readonly ContentType ImageGif = new("image/gif");

    /// <summary>A download.</summary>
    public static readonly ContentType ApplicationForceDownload = new("application/force-download");

    /// <summary>Anything else - data.</summary>
    public static readonly ContentType ApplicationOctetStream = new("application/octet-stream");

    /// <summary>A MP4 audio file.</summary>
    public static readonly ContentType AudioMp4 = new("audio/mp4");

    /// <summary>A OGG audio file.</summary>
    public static readonly ContentType AudioOgg = new("audio/ogg");

    /// <summary>A MPEG audio file.</summary>
    public static readonly ContentType AudioMpeg = new("audio/mpeg");

    /// <summary>A TIFF image.</summary>
    public static readonly ContentType ImageTiff = new("image/tiff");

    /// <summary>A CSV file.</summary>
    public static readonly ContentType TextCsv = new("text/csv");

    /// <summary>A RTF file.</summary>
    public static readonly ContentType TextRichText = new("text/richtext");

    /// <summary>Plain text.</summary>
    public static readonly ContentType TextPlain = new("text/plain");

    /// <summary>A XML file.</summary>
    public static readonly ContentType TextXml = new("text/xml");

    /// <summary>A XML file.</summary>
    public static readonly ContentType ApplicationXml = new("application/xml");

    /// <summary>A JavaScript file.</summary>
    public static readonly ContentType TextJavaScript = new("text/javascript");

    /// <summary>A SSE stream.</summary>
    public static readonly ContentType TextEventStream = new("text/event-stream");

    /// <summary>A uncompressed audio file.</summary>
    public static readonly ContentType AudioWav = new("audio/wav");

    /// <summary>Word processing document (e.g. docx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentWordProcessing = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

    /// <summary>A presentation (e.g. pptx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentPresentation = new("application/vnd.openxmlformats-officedocument.presentationml.presentation");

    /// <summary>A slideshow (e.g. .ppsx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentSlideshow = new("application/vnd.openxmlformats-officedocument.presentationml.slideshow");

    /// <summary>A sheet (e.g. .xlsx).</summary>
    public static readonly ContentType ApplicationOfficeDocumentSheet = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    /// <summary>An icon.</summary>
    public static readonly ContentType ImageIcon = new("image/x-icon");

    /// <summary>Microsoft embedded OTF.</summary>
    public static readonly ContentType FontEmbeddedOpenTypeFont = new("font/eot");

    /// <summary>True type font.</summary>
    public static readonly ContentType FontTrueTypeFont = new("font/ttf");

    /// <summary>Woff font.</summary>
    public static readonly ContentType FontWoff = new("font/woff");

    /// <summary>Woff2 font.</summary>
    public static readonly ContentType FontWoff2 = new("font/woff2");

    /// <summary>Open type font.</summary>
    public static readonly ContentType FontOpenTypeFont = new("font/otf");

    /// <summary>SVG.</summary>
    public static readonly ContentType ImageScalableVectorGraphics = new("image/svg");

    /// <summary>SVG XML.</summary>
    public static readonly ContentType ImageScalableVectorGraphicsXml = new("image/svg+xml");

    /// <summary>Compressed SVG.</summary>
    public static readonly ContentType ImageScalableVectorGraphicsCompressed = new("image/svgz");

    /// <summary>Form encoded.</summary>
    public static readonly ContentType ApplicationWwwFormUrlEncoded = new("application/x-www-form-urlencoded");

    /// <summary>Protobuf.</summary>
    public static readonly ContentType ApplicationProtobuf = new("application/protobuf");

    /// <summary>PDF.</summary>
    public static readonly ContentType ApplicationPdf = new("application/pdf");

    /// <summary>3GPP video.</summary>
    public static readonly ContentType Video3Gpp = new("video/3gpp");

    /// <summary>3GPP2 video.</summary>
    public static readonly ContentType Video3Gpp2 = new("video/3gpp2");

    /// <summary>AV1 video.</summary>
    public static readonly ContentType VideoAV1 = new("video/av1");

    /// <summary>AVC video.</summary>
    public static readonly ContentType VideoAvc = new("video/av");

    /// <summary>DV video.</summary>
    public static readonly ContentType VideoDV = new("video/dv");

    /// <summary>H261 video.</summary>
    public static readonly ContentType VideoH261 = new("video/H261");

    /// <summary>H263 video.</summary>
    public static readonly ContentType VideoH263 = new("video/H263");

    /// <summary>H264 video.</summary>
    public static readonly ContentType VideoH264 = new("video/H264");

    /// <summary>H265 video.</summary>
    public static readonly ContentType VideoH265 = new("video/H265");

    /// <summary>H266 video.</summary>
    public static readonly ContentType VideoH266 = new("video/H266");

    /// <summary>Matroska.</summary>
    public static readonly ContentType VideoMatroska = new("video/matroska");

    /// <summary>Matroska 3D.</summary>
    public static readonly ContentType VideoMatroska3D = new("video/matroska-3d");

    /// <summary>MJ2.</summary>
    public static readonly ContentType VideoMJ2 = new("video/mj2");

    /// <summary>MP4 video.</summary>
    public static readonly ContentType VideoMP4 = new("video/mp4");

    /// <summary>MPEG video.</summary>
    public static readonly ContentType VideoMpeg = new("video/mpeg");

    /// <summary>MPEG4 generic.</summary>
    public static readonly ContentType VideoMpeg4Generic = new("video/mpeg4-generic");

    /// <summary>MPV.</summary>
    public static readonly ContentType VideoMpv = new("video/MPV");

    /// <summary>Quicktime.</summary>
    public static readonly ContentType VideoQuicktime = new("video/quicktime");

    /// <summary>Raw video.</summary>
    public static readonly ContentType VideoRaw = new("video/raw");

    /// <summary>VC1.</summary>
    public static readonly ContentType VideoVC1 = new("video/vc1");

    /// <summary>VC2.</summary>
    public static readonly ContentType VideoVC2 = new("video/vc2");

    /// <summary>VP8.</summary>
    public static readonly ContentType VideoVP8 = new("video/VP8");

    /// <summary>VP9.</summary>
    public static readonly ContentType VideoVP9 = new("video/VP9");

    /// <summary>WebM.</summary>
    public static readonly ContentType VideoWebM = new("video/webm");

    #endregion

}
