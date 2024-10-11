using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

internal static class Adjustments
{

    /// <summary>
    /// Allows to chain the execution of the given adjustments into
    /// the given response builder.
    /// </summary>
    /// <param name="builder">The response builder to be adjusted</param>
    /// <param name="adjustments">The adjustments to be executed (if any)</param>
    /// <returns>The response builder to be chained</returns>
    internal static IResponseBuilder Adjust(this IResponseBuilder builder, Action<IResponseBuilder>? adjustments)
    {
        adjustments?.Invoke(builder);

        return builder;
    }
}
