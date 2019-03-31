using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Infrastructure;

namespace GenHTTP.Core.Protocol
{

    internal class RequestContentParser
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

        public async Task<Stream> GetBody(RequestBuffer buffer, NetworkStream inputStream)
        {
            var body = (Length > Configuration.RequestMemoryLimit) ? TemporaryFileStream.Create() : new MemoryStream((int)Length);

            var toFetch = Length - await buffer.Migrate(body, Length);

            while (toFetch > 0)
            {
                toFetch -= await Migrate(inputStream, body);
            }

            body.Seek(0, SeekOrigin.Begin);

            return body;
        }
        
        private async Task<int> Migrate(NetworkStream source, Stream target)
        {
            var buffer = new byte[Configuration.TransferBufferSize];

            var read = await source.ReadWithTimeoutAsync(buffer, 0, buffer.Length);

            await target.WriteAsync(buffer, 0, read);

            return read;
        }

        #endregion

    }

}
