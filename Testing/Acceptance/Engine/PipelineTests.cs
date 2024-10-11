using System.Net.Sockets;
using System.Text;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class PipelineTests
{

    [TestMethod]
    public void ServerSupportsPipelining()
    {
        using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

        using var client = new TcpClient("127.0.0.1", runner.Port)
        {
            ReceiveTimeout = 1000
        };

        var stream = client.GetStream();

        const int count = 10;

        WriteRequests(stream, count);

        ReadRequests(stream, count, "Hello World!");
    }

    private static void WriteRequests(Stream stream, int count)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true);

        var builder = new StringBuilder();

        for (var i = 0; i < count; i++)
        {
            builder.Append("GET / HTTP/1.1\r\n");
            builder.Append("Host: 128.0.0.1\r\n");
            builder.Append("Connection: Keep-Alive\r\n\r\n");
        }

        writer.Write(builder.ToString());

        writer.Flush();
    }

    private static void ReadRequests(Stream stream, int count, string searchFor)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);

        string? line;

        var found = 0;

        var result = new StringBuilder();

        try
        {
            while ((line = reader.ReadLine()) is not null)
            {
                if (line.Contains(searchFor))
                {
                    found++;
                }

                result.AppendLine(line);
            }
        }
        catch (IOException) { }

        Assert.AreEqual(count - 1, found); // last body does not end with \r\n
    }
}
