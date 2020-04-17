using GenHTTP.Api.Content;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Handlers implementing this interface will be queried when a content provider
    /// tries to resolve a route to another handler.
    /// </summary>
    public interface IHandlerResolver
    {

        /// <summary>
        /// Returns the handler responsible for the given route segment, if known.
        /// </summary>
        /// <param name="segment">The beginning of the routing target. Might be an alias.</param>
        /// <returns>The requested handler, if known</returns>
        IHandler? Find(string segment);

    }

}
