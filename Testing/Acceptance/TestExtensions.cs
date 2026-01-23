using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance;

public static class TestExtensions
{
    
    public static ValueTask<IResponse?> BuildTask(this IResponseBuilder builder) => new(builder.Build());

    public static IHandlerBuilder<HandlerBuilder> Wrap(this IHandler handler) => new HandlerBuilder(handler);

    public static string? GetETag(this HttpResponseMessage response) => response.GetHeader("ETag");

    public static DateTime WithoutMs(this DateTime date) => new(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
    
}
