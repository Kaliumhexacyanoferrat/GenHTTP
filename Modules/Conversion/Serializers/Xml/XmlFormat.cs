using System.Xml.Serialization;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Conversion.Serializers.Xml;

public sealed class XmlFormat : ISerializationFormat
{

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type)
    {
        var result = new XmlSerializer(type).Deserialize(stream);

        return new ValueTask<object?>(result);
    }

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
        var result = request.Respond()
                            .Content(new XmlContent(response))
                            .Type(ContentType.ApplicationXml);

        return new ValueTask<IResponseBuilder>(result);
    }

    public ValueTask<ReadOnlyMemory<byte>> SerializeAsync(object data)
    {
        return ByteStreamSerialization.SerializeAsync(b =>
        {
            var content = new XmlContent(data);

            return content.WriteAsync(b, 8192);
        });
    }

}
