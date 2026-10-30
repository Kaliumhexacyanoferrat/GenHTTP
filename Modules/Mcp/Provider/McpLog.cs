using Microsoft.Extensions.Logging;

namespace GenHTTP.Modules.Mcp.Provider;

internal static partial class McpLog
{

    [LoggerMessage(Level = LogLevel.Information, Message = "MCP {Method} answered in {Elapsed:0.##} ms (protocol {Version})")]
    internal static partial void OperationHandled(this ILogger logger, string method, double elapsed, string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "MCP {Method} from {Client} {ClientVersion} answered in {Elapsed:0.##} ms (protocol {Version})")]
    internal static partial void ClientHandled(this ILogger logger, string method, string client, string clientVersion, double elapsed, string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "MCP tool {Tool} succeeded in {Elapsed:0.##} ms")]
    internal static partial void ToolSucceeded(this ILogger logger, string tool, double elapsed);

    [LoggerMessage(Level = LogLevel.Information, Message = "MCP tool {Tool} returned an error in {Elapsed:0.##} ms: {Reason}")]
    internal static partial void ToolFailed(this ILogger logger, string tool, double elapsed, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "MCP tool {Tool} failed unexpectedly after {Elapsed:0.##} ms")]
    internal static partial void ToolCrashed(this ILogger logger, Exception exception, string tool, double elapsed);

    [LoggerMessage(Level = LogLevel.Information, Message = "MCP request rejected with HTTP {Status} in {Elapsed:0.##} ms: {Reason}")]
    internal static partial void RequestRejected(this ILogger logger, int status, double elapsed, string reason);

    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP notification {Method} accepted")]
    internal static partial void NotificationAccepted(this ILogger logger, string method);

}
