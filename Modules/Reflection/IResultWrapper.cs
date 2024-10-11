using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Allows the framework to unwrap <see cref="Result{T}" />
/// instances.
/// </summary>
internal interface IResultWrapper
{

    /// <summary>
    /// The actual result to be returned.
    /// </summary>
    object? Payload { get; }

    /// <summary>
    /// Performs the configured modifications to the response
    /// on the given builder.
    /// </summary>
    /// <param name="builder">The response builder to manipulate</param>
    void Apply(IResponseBuilder builder);
}
