using System.Text;

namespace GenHTTP.Testing.Acceptance.Utilities;

public static class FileUtil
{

    public static void WriteText(string path, string content)
    {
        using var file = File.Create(path, 4096, FileOptions.WriteThrough);

        file.Write(Encoding.UTF8.GetBytes(content));
        file.Flush();
    }

    public static async ValueTask WriteTextAsync(string path, string content)
    {
        // Kestrel flushes its output buffers as soon as the content length sent in
        // the headers is reached, causing the test execution to continue and introducing
        // a race condition where the test writes again to the same file that is not
        // yet disposed by the server. As there is no way around this, we will be hacky here.
        await Task.Delay(100);

        await using var file = File.Create(path, 4096, FileOptions.WriteThrough);

        await file.WriteAsync(Encoding.UTF8.GetBytes(content));
        await file.FlushAsync();
    }

}
