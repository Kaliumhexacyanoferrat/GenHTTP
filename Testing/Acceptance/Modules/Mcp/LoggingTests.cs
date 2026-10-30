using System.Collections.Concurrent;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

[TestClass]
public sealed class LoggingTests
{

    [TestMethod]
    public async Task TestOperationsAreLogged()
    {
        var provider = new CollectingProvider();

        using var factory = LoggerFactory.Create(b => b.AddProvider(provider).SetMinimumLevel(LogLevel.Debug));

        await using var host = new TestHost(TestServer.Create().Build());

        host.Host.Logging(factory, logRequests: false);

        await host.StartAsync();

        await McpClient.Legacy(host, null).RequestAsync("initialize", new JsonObject
        {
            ["protocolVersion"] = "2025-06-18",
            ["clientInfo"] = new JsonObject { ["name"] = "claude-code", ["version"] = "2.0.0" }
        });

        await McpClient.Legacy(host, null).NotifyAsync("notifications/initialized");

        var client = McpClient.Modern(host);

        await client.RequestAsync("server/discover");
        await client.RequestAsync("tools/list");
        await client.CallAsync("add", new JsonObject { ["a"] = 1, ["b"] = 2 });
        await client.CallAsync("fail");
        await client.CallAsync("crash");
        await client.CallAsync("nope");
        await client.CallAsync("error_result");

        var entries = provider.Entries.Where(e => e.Category == "GenHTTP.Modules.Mcp").ToList();

        AssertLogged(entries, LogLevel.Information, "MCP initialize from claude-code 2.0.0 answered in", "(protocol 2025-06-18)");
        AssertLogged(entries, LogLevel.Debug, "MCP notification notifications/initialized accepted");
        AssertLogged(entries, LogLevel.Information, "MCP server/discover from test-client 1.2.3 answered in", "(protocol 2026-07-28)");
        AssertLogged(entries, LogLevel.Information, "MCP tools/list answered in", "(protocol 2026-07-28)");
        AssertLogged(entries, LogLevel.Information, "MCP tool add succeeded in");
        AssertLogged(entries, LogLevel.Information, "MCP tool fail returned an error in", "The date needs to be in the future");
        AssertLogged(entries, LogLevel.Error, "MCP tool crash failed unexpectedly after");
        AssertLogged(entries, LogLevel.Information, "MCP tool error_result returned an error in", "Not today");
        AssertLogged(entries, LogLevel.Information, "MCP request rejected with HTTP 200", "Unknown tool: nope");

        Assert.IsNotNull(entries.Single(e => e.Level == LogLevel.Error).Exception);
    }

    private static void AssertLogged(List<LogEntry> entries, LogLevel level, params string[] fragments)
    {
        Assert.IsTrue(entries.Any(e => e.Level == level && fragments.All(f => e.Message.Contains(f))),
                      $"Expected a {level} entry with '{string.Join("', '", fragments)}', got:\n{string.Join("\n", entries.Select(e => $"{e.Level}: {e.Message}"))}");
    }

    private sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

    private sealed class CollectingProvider : ILoggerProvider
    {

        internal ConcurrentQueue<LogEntry> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new CollectingLogger(categoryName, Entries);

        public void Dispose() { }

    }

    private sealed class CollectingLogger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
    {

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => entries.Enqueue(new LogEntry(category, logLevel, formatter(state, exception), exception));

    }

}
