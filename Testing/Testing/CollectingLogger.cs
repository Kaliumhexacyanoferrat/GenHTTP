using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Testing;

/// <summary>
/// Keeps the warnings and errors a test host logs, so they can be written to the output of the
/// test that caused them instead of the shared console of the test process (analysis only).
/// </summary>
internal sealed class CollectingLogger : ILoggerProvider, ILogger
{
    private readonly ConcurrentQueue<string> _entries = new();

    internal IEnumerable<string> Entries => _entries;

    public ILogger CreateLogger(string categoryName) => this;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var entry = $"{logLevel}: {formatter(state, exception)}";

        if (exception is not null)
        {
            entry += $" -> {exception.GetType().Name}: {exception.Message}";
        }

        _entries.Enqueue(entry);
    }

    public void Dispose()
    {

    }

}
