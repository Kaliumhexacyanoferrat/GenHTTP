using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Modules.Conversion.Serializers;

/// <summary>
/// Registers formats that can be used to serialize and
/// deserialize objects sent to or received from a
/// service oriented handler.
/// </summary>
public sealed class SerializationRegistry
{
    private static readonly ReadOnlyMemory<byte> ContentTypeHeader = "Content-Type"u8.ToArray();
    
    private static readonly ReadOnlyMemory<byte> AcceptHeader = "Accept"u8.ToArray();

    #region Initialization

    public SerializationRegistry(ContentType defaultType, Dictionary<ContentType, ISerializationFormat> formats)
    {
        Default = defaultType;
        Formats = formats;
    }

    #endregion

    #region Get-/Setters

    private ContentType Default { get; }

    public IReadOnlyDictionary<ContentType, ISerializationFormat> Formats { get; }

    #endregion

    #region Functionality

    /// <summary>
    /// Analyzes the headers of the given request to determine the format
    /// that should be used to attempt to deserialize the request content.
    /// </summary>
    /// <param name="request">The request to be analyzed</param>
    /// <returns>A serialization format to deserialize the specified content type, or the default one (if any)</returns>
    public ISerializationFormat? GetDeserialization(IRequest request)
    {
        var contentType = request.Raw.Header.Headers.GetEntry(ContentTypeHeader);
        
        if (contentType != null)
        {
            return GetFormat(new ContentType(contentType.Value));
        }

        return GetFormat(Default);
    }

    /// <summary>
    /// Analyzes the given request to determine the format preference for serialized content
    /// and attempts to provide a format that matches this preference.
    /// </summary>
    /// <param name="request">The request to be analyzed</param>
    /// <returns>Either a format that can serialize into the requested format or the default format (if any)</returns>
    public ISerializationFormat? GetSerialization(IRequest request)
    {
        var accepted = request.Raw.Header.Headers.GetEntry(AcceptHeader);
        
        if (accepted != null)
        {
            return GetFormat(new ContentType(accepted.Value)) ?? GetFormat(Default);
        }

        return GetFormat(Default);
    }

    /// <summary>
    /// Attempts to provide a format that can be used to serialize and deserialize
    /// the given content type.
    /// </summary>
    /// <param name="contentType">The content type to check for</param>
    /// <returns>Either a format that can serialize into the requested format or the default format (if any)</returns>
    public ISerializationFormat? GetFormat(string? contentType)
    {
        if (contentType != null)
        {
            return GetFormat(new ContentType(contentType));
        }

        return GetFormat(Default);
    }

    private ISerializationFormat? GetFormat(ContentType contentType) => Formats.GetValueOrDefault(contentType);

    #endregion

}
