using GenHTTP.Api.Protocol;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

public sealed class YamlFormat : ISerializationFormat
{
    private const int MaximumDepth = 32;

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

        var parser = BufferedParser.Read(new Parser(reader), MaximumDepth);

        return new(Deserializer.Deserialize(parser, type));
    }

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
        var result = request.Respond()
                            .Content(new YamlContent(response));

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
