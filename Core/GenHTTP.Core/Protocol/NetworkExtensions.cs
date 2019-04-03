using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core.Protocol
{

    internal static class NetworkExtensions
    {

        internal static async Task<int> ReadWithTimeoutAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            var result = 0;

            var task = Task.Run(async () => {
                result = await stream.ReadAsync(buffer, offset, count);
            });

            var success = await Task.WhenAny(task, Task.Delay(stream.ReadTimeout)) == task;

            if (!success)
            {
                return -1;
            }

            if (result == 0)
            {
                throw new NetworkException("Read 0 bytes from input stream");
            }
            
            return result;
        }

    }

}
