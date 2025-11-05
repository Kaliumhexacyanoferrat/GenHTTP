using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class ResponseModificationExtensions
{

    /// <summary>
    /// Applies the given modifications to the response.
    /// </summary>
    /// <param name="builder">The response to be modified</param>
    /// <param name="modifications">The modifications to be applied</param>
    public static IResponseBuilder Apply(this IResponseBuilder builder, Action<IResponseBuilder>? modifications)
    {
        modifications?.Invoke(builder);
        return builder;
    }

}
