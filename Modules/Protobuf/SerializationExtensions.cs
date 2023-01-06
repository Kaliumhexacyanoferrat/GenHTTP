using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Protobuf.Providers;

namespace GenHTTP.Modules.Protobuf
{
    public static class SerializationExtensions
    {
        public static SerializationBuilder AddProtobuf(this SerializationBuilder serializationBuilder)
        {
            return serializationBuilder.Add(ContentType.ApplicationProtobuf, new ProtobufFormat());
        }
    }
}
