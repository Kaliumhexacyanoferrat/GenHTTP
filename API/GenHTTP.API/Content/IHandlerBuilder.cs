namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Allows to create a handler instance.
    /// </summary>
    public interface IHandlerBuilder 
    {

        /// <summary>
        /// Creates the configured handler instance.
        /// </summary>
        /// <param name="parent">The parent of the handler to be created</param>
        /// <returns>The newly created handler instance</returns>
        IHandler Build(IHandler parent);
    
    }

    public interface IHandlerBuilder<TBuilder> : IHandlerBuilder where TBuilder : IHandlerBuilder<TBuilder>
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

}
