using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class LoggingTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestsAreLoggedByDefault(TestEngine engine)
    {
        var factory = new CapturingLoggerFactory();

        await using var runner = new TestHost(Layout.Create().Index(new OkHandler()).Build(), engine: engine);

        await runner.Host.Logging(factory).StartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.HasCount(1, factory.Messages);

        var entry = factory.Messages[0];

        Assert.Contains("GET", entry);
        Assert.Contains("200", entry);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOutermostConcernObservesNotFound(TestEngine engine)
    {
        var factory = new CapturingLoggerFactory();

        await using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        await runner.Host.Logging(factory).StartAsync();

        using var response = await runner.GetResponseAsync("/does/not/exist");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);

        Assert.HasCount(1, factory.Messages);

        var entry = factory.Messages[0];

        Assert.Contains("404", entry);
        Assert.Contains("/does/not/exist", entry);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLoggingCanBeDisabled(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Index(new OkHandler()).Build(), engine: engine);

        await runner.Host.Logging(NullLoggerFactory.Instance).StartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsInstanceOfType<NullLoggerFactory>(runner.Host.Instance!.Logging);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNullLoggerFactoryAlsoDisablesLogging(TestEngine engine)
    {
        await using var runner = new TestHost(Layout.Create().Index(new OkHandler()).Build(), engine: engine);

        await runner.Host.Logging(NullLoggerFactory.Instance).StartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsInstanceOfType<NullLoggerFactory>(runner.Host.Instance!.Logging);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestLoggingCanBeDisabledWhileKeepingInfrastructure(TestEngine engine)
    {
        var factory = new CapturingLoggerFactory();

        await using var runner = new TestHost(Layout.Create().Index(new OkHandler()).Build(), engine: engine);

        await runner.Host.Logging(factory, logRequests: false).StartAsync();

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        // no per-request entries, but the factory itself is still wired up
        // and used for the server's own lifecycle/endpoint logging
        Assert.IsEmpty(factory.Messages);
        Assert.IsTrue(factory.AllMessages.Any(m => m.Contains("started")));

        Assert.AreSame(factory, runner.Host.Instance!.Logging);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServerLifecycleIsLogged(TestEngine engine)
    {
        var factory = new CapturingLoggerFactory();

        await using var runner = new TestHost(Layout.Create().Index(new OkHandler()).Build(), engine: engine);

        await runner.Host.Logging(factory).StartAsync();

        Assert.IsTrue(factory.AllMessages.Any(m => m.Contains("started")));
        Assert.IsTrue(factory.AllMessages.Any(m => m.Contains("Listening")));

        await runner.Host.StopAsync();

        Assert.IsTrue(factory.AllMessages.Any(m => m.Contains("shutting down")));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHeaderIsCapturedBeforeBodyIsReleased(TestEngine engine)
    {
        var factory = new CapturingLoggerFactory();

        await using var runner = new TestHost(Layout.Create().Index(new ReleasingHandler()).Build(), engine: engine);

        await runner.Host.Logging(factory).StartAsync();

        var request = runner.GetRequest();
        request.Headers.ConnectionClose = true;

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.HasCount(1, factory.Messages);

        Assert.Contains("GET", factory.Messages[0]);
    }

    #endregion

    #region Supporting types

    private sealed class OkHandler : IHandler
    {

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request) =>
            ValueTask.FromResult<IResponse?>(request.Respond().Status(ResponseStatus.Ok).Build());

    }

    private sealed class ReleasingHandler : IHandler
    {

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            request.GetBody(HeaderAccess.Release);

            return ValueTask.FromResult<IResponse?>(request.Respond().Status(ResponseStatus.Ok).Build());
        }

    }

    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        private const string RequestCategory = "GenHTTP.Requests";

        private readonly List<(string Category, string Message)> _entries = [];

        /// <summary>
        /// Only the entries written by the request logging concern, ignoring
        /// the server and endpoint lifecycle messages that share this factory.
        /// </summary>
        public List<string> Messages => _entries.Where(e => e.Category == RequestCategory).Select(e => e.Message).ToList();

        public List<string> AllMessages => _entries.Select(e => e.Message).ToList();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _entries);

        public void AddProvider(ILoggerProvider provider)
        {
            // not needed for testing purposes
        }

        public void Dispose()
        {
            // not needed for testing purposes
        }

    }

    private sealed class CapturingLogger(string category, List<(string Category, string Message)> entries) : ILogger
    {

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            entries.Add((category, formatter(state, exception)));
        }

    }

    #endregion

}
