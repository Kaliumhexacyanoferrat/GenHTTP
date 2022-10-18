using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Engine.Infrastructure.Configuration;

namespace GenHTTP.Engine.Protocol
{

    /// <summary>
    /// Efficiently reads the body from the HTTP request, storing it
    /// in a temporary file if it exceeds the buffering limits.
    /// </summary>
    internal sealed class RequestContentParser
    {

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

        internal async Task<Stream> GetBody(RequestBuffer buffer)
        {
            var body = (Length > Configuration.RequestMemoryLimit) ? TemporaryFileStream.Create() : new MemoryStream((int)Length);

            var toFetch = Length;

            while (toFetch > 0)
            {
                await buffer.Read().ConfigureAwait(false);

                var toRead = Math.Min(buffer.Data.Length, Math.Min(Configuration.TransferBufferSize, toFetch));

                if (toRead == 0)
                {
                    throw new InvalidOperationException($"No data read from the transport but {toFetch} bytes are remaining");
                }

                var data = buffer.Data.Slice(0, toRead);

                var position = data.GetPosition(0);

                while (data.TryGet(ref position, out var memory))
                {
                    await body.WriteAsync(memory);
                }

                buffer.Advance(toRead);

                toFetch -= toRead;
            }

            body.Seek(0, SeekOrigin.Begin);

            return body;
        }

        #endregion

    }

}
