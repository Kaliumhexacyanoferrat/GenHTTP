using System;
using System.IO;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;

using Newtonsoft.Json;

namespace GenHTTP.Modules.Webservices.Formats
{

    public class JsonFormat : ISerializationFormat
    {

        public object Deserialize(Stream stream, Type type)
        {
            var streamReader = new StreamReader(stream);

            var jsonReader = new JsonTextReader(streamReader);

            var serializer = new JsonSerializer();

            return serializer.Deserialize(jsonReader, type)!;
        }

        public IResponseBuilder Serialize(IRequest request, object response)
        {
            return request.Respond()
                          .Content(new JsonContent(response))
                          .Type(ContentType.ApplicationJson);
        }

    }

}
