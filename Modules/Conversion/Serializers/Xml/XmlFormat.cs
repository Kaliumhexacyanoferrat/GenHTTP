using System.Xml.Serialization;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Xml;

public sealed class XmlFormat : ISerializationFormat
{

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type)
    {
        var result = new XmlSerializer(type).Deserialize(stream);

        return new ValueTask<object?>(result);
    }

    public ValueTask<IResponseBuilder> SerializeAsync<T>(IRequest request, T response) where T : class
    {
        var result = request.Respond()
                            .Content(new XmlContent(response));

        return new ValueTask<IResponseBuilder>(result);
    }

    public ValueTask<ReadOnlyMemory<byte>> SerializeAsync<T>(T data) where T : class
    {
        return ByteStreamSerialization.SerializeAsync(b =>
        {
            var serializer = new XmlSerializer(data.GetType());

            serializer.Serialize(b, data);

            return ValueTask.CompletedTask;
        });
    }

}
