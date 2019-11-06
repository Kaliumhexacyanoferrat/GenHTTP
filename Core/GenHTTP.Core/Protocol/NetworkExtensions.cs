using System.IO;
using System.Threading.Tasks;

namespace GenHTTP.Core.Protocol
{

    internal static class NetworkExtensions
    {

        internal static async Task<int> ReadWithTimeoutAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            var result = 0;

            var task = Task.Run(async () =>
            {
                result = await stream.ReadAsync(buffer, offset, count);
            });

            var success = await Task.WhenAny(task, Task.Delay(stream.ReadTimeout)) == task;

            if (!success || result == 0)
            {
                return -1;
            }

            return result;
        }

    }

}
