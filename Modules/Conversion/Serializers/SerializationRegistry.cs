using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers;

/// <summary>
/// Registers formats that can be used to serialize and
/// deserialize objects sent to or received from a
/// service oriented handler.
/// </summary>
public sealed class SerializationRegistry
{

    #region Initialization

    public SerializationRegistry(FlexibleContentType defaultType, Dictionary<FlexibleContentType, ISerializationFormat> formats)
    {
        Default = defaultType;
        Formats = formats;
    }

    #endregion

    #region Get-/Setters

    private FlexibleContentType Default { get; }

    public IReadOnlyDictionary<FlexibleContentType, ISerializationFormat> Formats { get; }

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
        if (request.Headers.TryGetValue("Content-Type", out var requested))
        {
            return GetFormat(FlexibleContentType.Parse(requested));
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
        if (request.Headers.TryGetValue("Accept", out var accepted))
        {
            return GetFormat(FlexibleContentType.Parse(accepted)) ?? GetFormat(Default);
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
            return GetFormat(FlexibleContentType.Parse(contentType));
        }

        return GetFormat(Default);
    }

    private ISerializationFormat? GetFormat(FlexibleContentType contentType) => Formats.GetValueOrDefault(contentType);

    #endregion

}
