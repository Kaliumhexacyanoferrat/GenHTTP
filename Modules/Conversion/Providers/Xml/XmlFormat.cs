using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Conversion.Providers.Xml
{

    public class XmlFormat : ISerializationFormat
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
                                .Type(ContentType.TextXml);

            return new ValueTask<IResponseBuilder>(result);
        }

    }

}
