﻿using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers;

/// <summary>
/// Registers formats that can be used to serialize and
/// deserialize objects sent to or received from a
/// service oriented handler.
/// </summary>
public sealed class SerializationRegistry
{

    #region Initialization

    public SerializationRegistry(FlexibleContentType defaultType,
        Dictionary<FlexibleContentType, ISerializationFormat> formats)
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

    public ISerializationFormat? GetDeserialization(IRequest request)
    {
        if (request.Headers.TryGetValue("Content-Type", out var requested))
        {
            return GetFormat(FlexibleContentType.Parse(requested));
        }

        return GetFormat(Default);
    }

    public ISerializationFormat? GetSerialization(IRequest request)
    {
        if (request.Headers.TryGetValue("Accept", out var accepted))
        {
            return GetFormat(FlexibleContentType.Parse(accepted)) ?? GetFormat(Default);
        }

        return GetFormat(Default);
    }

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
