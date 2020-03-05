using System;
using System.IO;
using System.Xml.Serialization;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;

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
            return request.Respond()
                          .Content(new XmlContent(response))
                          .Type(ContentType.TextXml);
        }

    }

}
