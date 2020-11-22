using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using PooledAwait;

namespace GenHTTP.Modules.Conversion.Providers.Json
{

    public sealed class JsonContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => null;

        private object Data { get; }

        private JsonSerializerOptions Options { get; }

        #endregion

        #region Initialization

        public JsonContent(object data, JsonSerializerOptions options)
        {
            Data = data;
            Options = options;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync()
        {
            return new ValueTask<ulong?>((ulong)Data.GetHashCode());
        }

        public ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            return DoWrite(this, target);

            static async PooledValueTask DoWrite(JsonContent self, Stream target)
            {
                await JsonSerializer.SerializeAsync(target, self.Data, self.Data.GetType(), self.Options);
            }
        }

        #endregion

    }

}
