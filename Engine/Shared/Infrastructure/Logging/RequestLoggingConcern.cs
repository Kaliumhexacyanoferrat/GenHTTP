using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Shared.Infrastructure.Logging;

/// <summary>
/// Logs the method, path, status code, content length and duration of every
/// request that passed through the handler chain.
/// </summary>
/// <remarks>
/// Captures the request method and path before invoking the inner handler, as
/// handlers may release the header information by calling GetBody() with
/// <see cref="HeaderAccess.Release" />.
/// </remarks>
public sealed class RequestLoggingConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private ILogger? Logger { get; set; }

    #endregion

    #region Initialization

    public RequestLoggingConcern(IHandler content)
    {
        Content = content;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server)
    {
        Logger = server.Logging.CreateLogger("GenHTTP.Requests");

        return Content.PrepareAsync(server);
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var logger = Logger;

        if (logger is not null && logger.IsEnabled(LogLevel.Information))
        {
            return HandleAndLogAsync(request, logger);
        }

        return Content.HandleAsync(request);
    }

    private async ValueTask<IResponse?> HandleAndLogAsync(IRequest request, ILogger logger)
    {
        var header = request.Header;

        var method = header.Method.ToString();
        var path = header.Path.ToString();

        var start = Stopwatch.GetTimestamp();

        var response = await Content.HandleAsync(request);

        var elapsed = Stopwatch.GetElapsedTime(start);

        logger.RequestHandled(method, path, response != null ? (int)response.Status : 404, response?.Content?.Length ?? 0, elapsed.TotalMilliseconds);

        return response;
    }

    #endregion

}
