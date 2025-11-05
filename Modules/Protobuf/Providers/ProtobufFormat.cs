using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
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

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
        var result = request.Respond()
                            .Content(new ProtobufContent(response))
                            .Type(ContentType.ApplicationProtobuf);

        return new ValueTask<IResponseBuilder>(result);
    }
}
