using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace GenHTTP.Core.Protocol
{

    internal static class NetworkExtensions
    {

        internal static async ValueTask<ReadResult?> ReadWithTimeoutAsync(this PipeReader reader, TimeSpan timeout)
        {
            ReadResult? result = null;

            var task = Task.Run(async () =>
            {
                result = await reader.ReadAsync();
            });

            var success = await Task.WhenAny(task, Task.Delay(timeout)) == task;

            if (!success || (result == null))
            {
                return null;
            }

            return result;
        }

    }

}
