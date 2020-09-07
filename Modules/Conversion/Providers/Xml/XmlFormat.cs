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

        public Task<object> Deserialize(Stream stream, Type type)
        {
            var result = new XmlSerializer(type).Deserialize(stream);

            return Task.FromResult(result);
        }

        public IResponseBuilder Serialize(IRequest request, object response)
        {
            return request.Respond()
                          .Content(new XmlContent(response))
                          .Type(ContentType.TextXml);
        }

    }

}
