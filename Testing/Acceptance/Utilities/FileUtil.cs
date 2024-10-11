using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            using var file = File.Create(path, 4096, FileOptions.WriteThrough);

            await file.WriteAsync(Encoding.UTF8.GetBytes(content));
            await file.FlushAsync();
        }

}
