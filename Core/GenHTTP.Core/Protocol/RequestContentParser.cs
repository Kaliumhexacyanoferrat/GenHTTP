﻿using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Core.Infrastructure.Configuration;

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

        internal async Task<Stream> GetBody(RequestBuffer buffer)
        {
            var body = (Length > Configuration.RequestMemoryLimit) ? TemporaryFileStream.Create() : new MemoryStream((int)Length);

            var toFetch = Length;

            while (toFetch > 0)
            {
                await buffer.Read();

                var toRead = Math.Min(buffer.Data.Length, Math.Min(Configuration.TransferBufferSize, toFetch));

                var data = buffer.Data.Slice(0, toRead);

                var position = data.GetPosition(0);

                while (data.TryGet(ref position, out var memory))
                {
                    body.Write(memory.Span);
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
