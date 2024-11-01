using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

public sealed class YamlFormat : ISerializationFormat
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                                                  .Build();

    #region Functionality

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);

        return new(Deserializer.Deserialize(reader, type));
    }

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
        var result = request.Respond()
                            .Content(new YamlContent(response))
                            .Type(ContentType.ApplicationYaml);

        return new ValueTask<IResponseBuilder>(result);
    }

    #endregion

}
