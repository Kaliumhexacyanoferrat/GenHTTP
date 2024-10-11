using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Conversion.Serializers.Xml;

namespace GenHTTP.Modules.Conversion;

/// <summary>
/// Entry point to configure the formats supported by a webservice
/// resource.
/// </summary>
public static class Serialization
{

    /// <summary>
    /// Returns a registry that will support JSON and XML serialization
    /// and will use JSON as a default format.
    /// </summary>
    public static SerializationBuilder Default() => new SerializationBuilder().Default(ContentType.ApplicationJson)
                                                                              .Add(ContentType.ApplicationJson, new JsonFormat())
                                                                              .Add(ContentType.ApplicationWwwFormUrlEncoded, new FormFormat())
                                                                              .Add(ContentType.TextXml, new XmlFormat());

    /// <summary>
    /// Returns an empty registry to be customized.
    /// </summary>
    public static SerializationBuilder Empty() => new();

}
