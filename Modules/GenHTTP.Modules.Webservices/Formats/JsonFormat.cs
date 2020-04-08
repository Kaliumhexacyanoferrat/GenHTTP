using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;


namespace GenHTTP.Modules.Webservices.Formats
{

    public class JsonFormat : ISerializationFormat
    {
        private static readonly JsonSerializerOptions OPTIONS = new JsonSerializerOptions()
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<object> Deserialize(Stream stream, Type type)
        {
            return await JsonSerializer.DeserializeAsync(stream, type, OPTIONS);
        }

        public IResponseBuilder Serialize(IRequest request, object response)
        {
            return request.Respond()
                          .Content(new JsonContent(response, OPTIONS))
                          .Type(ContentType.ApplicationJson);
        }

    }

}
