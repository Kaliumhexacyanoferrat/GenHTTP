using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using ProtoBuf;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using GenHTTP.Modules.Conversion.Serializers;

namespace GenHTTP.Modules.Protobuf.Providers;

public sealed class ProtobufFormat : ISerializationFormat
{
    public ValueTask<object?> DeserializeAsync(Stream stream, [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] Type type)
    {
            object deserializedObject = Serializer.Deserialize(type, stream);
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
