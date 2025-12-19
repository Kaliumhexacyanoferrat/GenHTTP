using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using YamlDotNet.Serialization;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

public sealed class YamlFormat : ISerializationFormat
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(new IgnoreCaseNamingConvention())
                                                                                  .Build();

    #region Supporting data structures

    private sealed class IgnoreCaseNamingConvention : INamingConvention
    {
        public string Apply(string value) => value.ToLowerInvariant();

        public string Reverse(string value) => throw new NotImplementedException();
    }

    #endregion

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

    public ValueTask<ReadOnlyMemory<byte>> SerializeAsync(object data)
    {
        return ByteStreamSerialization.SerializeAsync(b =>
        {
            var content = new YamlContent(data);

            return content.WriteAsync(b, 8192);
        });
    }

    #endregion

}
