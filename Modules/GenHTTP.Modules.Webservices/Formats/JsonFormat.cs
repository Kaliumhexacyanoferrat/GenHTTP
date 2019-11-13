using System;
using System.IO;

using GenHTTP.Api.Protocol;

using Newtonsoft.Json;

namespace GenHTTP.Modules.Webservices.Formats
{

    public class JsonFormat : ISerializationFormat
    {

        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public object Deserialize(Stream stream, Type type)
        {
            var streamReader = new StreamReader(stream);

            var jsonReader = new JsonTextReader(streamReader);

            var serializer = new JsonSerializer();

            return serializer.Deserialize(jsonReader, type)!;
        }

        public IResponseBuilder Serialize(IRequest request, object response)
        {
            var memory = new MemoryStream();

            var streamWriter = new StreamWriter(memory);

            var jsonWriter = new JsonTextWriter(streamWriter);

            var serializer = JsonSerializer.Create(SETTINGS);

            serializer.Serialize(jsonWriter, response);

            jsonWriter.Flush();

            memory.Seek(0, SeekOrigin.Begin);

            return request.Respond().Content(memory, ContentType.ApplicationJson);
        }

    }

}
