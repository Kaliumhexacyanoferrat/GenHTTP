using System.Buffers;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace genhttp.Tests;

public class Upload
{

    [ResourceMethod(Method.Post)]
    public async ValueTask<long> Compute(Stream input)
    {
        var pool = ArrayPool<byte>.Shared;

        var buffer = pool.Rent(16384);

        try
        {
            long total = 0;

            var read = 0;

            while ((read = await input.ReadAsync(buffer)) > 0)
            {
                total += read;
            }

            return total;
        }
        finally
        {
            pool.Return(buffer);
        }
    }

}
