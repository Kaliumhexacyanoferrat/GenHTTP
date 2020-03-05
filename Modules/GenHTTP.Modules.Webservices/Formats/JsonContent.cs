using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using Newtonsoft.Json;

namespace GenHTTP.Modules.Webservices.Formats
{

    public class JsonContent : IResponseContent
    {
        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        #region Get-/Setters

        public ulong? Length => null;

        private object Data { get; }

        #endregion

        #region Initialization

        public JsonContent(object data)
        {
            Data = data;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            var streamWriter = new StreamWriter(target, Encoding.UTF8, (int)bufferSize, true);

            var jsonWriter = new JsonTextWriter(streamWriter);
            
            var serializer = JsonSerializer.Create(SETTINGS);

            serializer.Serialize(jsonWriter, Data);

            await jsonWriter.FlushAsync();

            await streamWriter.FlushAsync();
        }

        #endregion

    }

}
