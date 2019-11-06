using System.Buffers;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Protocol
{

    internal class RequestContentParser
    {
        private static ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        #region Get-/Setters

        internal long Length { get; }

        internal NetworkConfiguration Configuration { get; }

        #endregion

        #region Initialization

        internal RequestContentParser(long length, NetworkConfiguration configuration)
        {
            Length = length;
            Configuration = configuration;
        }

        #endregion

        #region Functionality

        public async Task<Stream> GetBody(RequestBuffer buffer, Stream inputStream)
        {
            var body = (Length > Configuration.RequestMemoryLimit) ? TemporaryFileStream.Create() : new MemoryStream((int)Length);

            var toFetch = Length - await buffer.Migrate(body, Length);

            while (toFetch > 0)
            {
                var read = await Migrate(inputStream, body);

                if (read > 0)
                {
                    toFetch -= read;
                }
                else
                {
                    throw new NetworkException("Failed to read body from stream");
                }
            }

            body.Seek(0, SeekOrigin.Begin);

            return body;
        }

        private async Task<int> Migrate(Stream source, Stream target)
        {
            var buffer = POOL.Rent((int)Configuration.TransferBufferSize);

            try
            {
                var read = await source.ReadWithTimeoutAsync(buffer, 0, buffer.Length);

                if (read > 0)
                {
                    await target.WriteAsync(buffer, 0, read);
                }

                return read;
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

    }

}
