using System;
using System.IO;
using System.Xml.Serialization;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Webservices.Formats
{

    public class XmlFormat : ISerializationFormat
    {

        public object Deserialize(Stream stream, Type type)
        {
            return new XmlSerializer(type).Deserialize(stream);
        }

        public IResponseBuilder Serialize(IRequest request, object response)
        {
            var stream = new MemoryStream();

            new XmlSerializer(response.GetType()).Serialize(stream, response);

            stream.Seek(0, SeekOrigin.Begin);

            return request.Respond().Content(stream, ContentType.TextXml);
        }

    }

}
