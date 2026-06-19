using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class RequestBodyExtensions
{

    /// <summary>
    /// Reads and parses the body as form encoded ("application/x-www-form-urlencoded") content.
    /// </summary>
    /// <param name="body">The body to be parsed</param>
    /// <returns>The arguments found within the body</returns>
    public static ValueTask<BodyArguments> AsBodyArgumentsAsync(this IRequestBody body) => BodyArguments.CreateAsync(body);

}
