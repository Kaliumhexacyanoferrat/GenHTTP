using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Serializers;

using ProtoBuf;

namespace GenHTTP.Modules.Protobuf.Providers;

public sealed class ProtobufFormat : ISerializationFormat
{
    public ValueTask<object?> DeserializeAsync(Stream stream, [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] Type type)
    {
        var deserializedObject = Serializer.Deserialize(type, stream);
        return new ValueTask<object?>(deserializedObject);
    }

    public ValueTask<IResponseBuilder> SerializeAsync<T>(IRequest request, T response) where T : class
    {
        var result = request.Respond()
                            .Content(new ProtobufContent(response));

        return new ValueTask<IResponseBuilder>(result);
    }

    public ValueTask<ReadOnlyMemory<byte>> SerializeAsync<T>(T data) where T : class
    {
        return ByteStreamSerialization.SerializeAsync(b =>
        {
            var content = new ProtobufContent(data);

            return content.WriteAsync(b, 8192);
        });
    }

}
