using System.Net.Sockets;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

public abstract class WireTest
{
    protected const string NL = "\r\n";

    protected static ValueTask TestAsync(string[] request, string assertion, IHandlerBuilder? handler = null) => TestAsync(string.Join(NL, request) + NL, assertion, handler);

    protected static async ValueTask TestAsync(string request, string assertion, IHandlerBuilder? handler = null)
    {
        await using var host = await TestHost.RunAsync(handler ?? Layout.Create());

        var result = await SendAsync(host, w =>
        {
            w.Write(request);
            w.Write(NL);
        });

        AssertX.Contains(assertion, result);
    }

    protected static async ValueTask<string> SendAsync(TestHost host, Action<StreamWriter> sender)
    {
        using var client = new TcpClient("127.0.0.1", host.Port)
        {
            ReceiveTimeout = 1000
        };

        var stream = client.GetStream();

        await using var writer = new StreamWriter(stream, leaveOpen: true);

        sender(writer);

        await writer.FlushAsync();

        using var reader = new StreamReader(stream, leaveOpen: true);

        return await reader.ReadToEndAsync();
    }

}
