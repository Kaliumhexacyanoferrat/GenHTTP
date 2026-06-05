using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.HttpArena;

public class UploadService
{
    [ResourceMethod(Method.Post)]
    public ValueTask<long> Compute(Stream input)
    {
        if (input.CanSeek)
        {
            return ValueTask.FromResult(input.Length);
        }

        return ComputeManually(input);
    }

    private async ValueTask<long> ComputeManually(Stream input)
    {
        var buffer = new byte[8192];
        long total = 0;
        int read;

        while ((read = await input.ReadAsync(buffer)) > 0)
        {
            total += read;
        }

        return total;
    }
}
