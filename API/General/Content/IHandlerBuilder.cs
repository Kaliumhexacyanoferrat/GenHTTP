namespace GenHTTP.Api.Content;

/// <summary>
/// Allows to create a handler instance.
/// </summary>
public interface IHandlerBuilder
{

    /// <summary>
    /// Creates the configured handler instance.
    /// </summary>
    /// <returns>The newly created handler instance</returns>
    IHandler Build();

}

/// <summary>
/// Typed version of the handler builder. Should be used by all handler
/// builder implementations to enable common features on all builders.
/// </summary>
/// <typeparam name="TBuilder">The type of builder, so that we can use the builder pattern</typeparam>
public interface IHandlerBuilder<out TBuilder> : IHandlerBuilder where TBuilder : IHandlerBuilder<TBuilder>
{

    /// <summary>
    /// Adds the given concern to the resulting handler.
    /// </summary>
    /// <remarks>
    /// The first concern added to the builder will be the new root
    /// of the chain returned by the builder.
    /// </remarks>
    /// <param name="concern">The concern to be added to the resulting handler</param>
    TBuilder Add(IConcernBuilder concern);

}
