using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class ResponseBuilderExtensions
{

    public static ValueTask<IResponse?> BuildTask(this IResponseBuilder builder) => new(builder.Build());

}
