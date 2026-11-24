using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using ioxide;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// A reactor that cannot start fails the server start with its reason, instead of ending the
/// process from the reactor's thread or leaving a server that listens one reactor short.
/// </summary>
[TestClass]
public sealed class ReactorStartupTests
{

    [IoxideTestMethod]
    public async Task TestARingTheKernelRefusesFailsTheStart()
    {
        // Every kernel refuses a ring this large, so this fails the same way an old kernel does.
        var options = new EngineOptions { Reactor = new ReactorOptions { ReactorCount = 2, RingEntries = 65_536 } };

        var server = Build(options: options);

        var e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await server.StartAsync());

        // ioxide names the cause, and the start passes it on
        StringAssert.Contains(e.Message, "32768");
        Assert.IsNotNull(e.InnerException);

        await server.StopAsync();
    }

    [IoxideTestMethod]
    public async Task TestAThrowFromOnReactorStartFailsTheStart()
    {
        var options = new EngineOptions { Reactor = new ReactorOptions { ReactorCount = 2 } };

        var server = Build(_ => throw new InvalidOperationException("The reactor callback failed"), options);

        var e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await server.StartAsync());

        StringAssert.Contains(e.Message, "The reactor callback failed");

        await server.StopAsync();
    }

    [IoxideTestMethod]
    public async Task TestAPortTakenAlreadyFailsTheStart()
    {
        var port = (ushort)TestHost.NextPort();

        // a listener without SO_REUSEPORT, which the reactors' listeners cannot share the port with
        using var taken = new TcpListener(IPAddress.Any, port);

        taken.Start();

        var options = new EngineOptions { Reactor = new ReactorOptions { ReactorCount = 2 } };

        var server = Build(options: options, port: port);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await server.StartAsync());

        await server.StopAsync();
    }

    [IoxideTestMethod]
    public async Task TestTheReactorsServeOnceStarted()
    {
        var options = new EngineOptions { Reactor = new ReactorOptions { ReactorCount = 2 } };

        var port = (ushort)TestHost.NextPort();

        var server = Build(options: options, port: port);

        await server.StartAsync();

        try
        {
            using var client = new HttpClient();

            Assert.AreEqual("ok", await client.GetStringAsync($"http://localhost:{port}/ok"));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestASizeTheReactorCannotRunWithIsRefusedBeforeItRuns()
    {
        // ioxide refuses this in the reactor's constructor, which the engine reaches before starting
        // any thread - so nothing is left half started to stop afterwards.
        var options = new EngineOptions { Tcp = new TcpTransportOptions { RecvQueueEntries = 100 } };

        var server = Build(options: options);

        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () => await server.StartAsync());

        await server.StopAsync();
    }

    private static IServerHost Build(Action<Reactor>? onReactorStart = null, EngineOptions? options = null, ushort? port = null)
        => Host.Create(onReactorStart, options)
               .Handler(Layout.Create().Add("ok", Content.From(Resource.FromString("ok"))))
               .Bind(IPAddress.Loopback, port ?? (ushort)TestHost.NextPort());

}
