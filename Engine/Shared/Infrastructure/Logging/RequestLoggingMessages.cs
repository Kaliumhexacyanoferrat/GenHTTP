using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Shared.Infrastructure.Logging;

internal static partial class RequestLoggingMessages
{

    [LoggerMessage(Level = LogLevel.Information, Message = "{Method} {Path} - HTTP {Status} - {Length} bytes - {Elapsed:0.##} ms")]
    public static partial void RequestHandled(this ILogger logger, string method, string path, int status, ulong length, double elapsed);

}
