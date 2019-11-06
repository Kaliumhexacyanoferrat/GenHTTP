using GenHTTP.Api.Modules;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Passed to <see cref="IRouter"/> instances to determine
    /// the content provider to be used to generate a response.
    /// </summary>
    public interface IEditableRoutingContext : IRoutingContext
    {

        /// <summary>
        /// Scopes the context to the given router. This method
        /// must be called by any router when handling the
        /// context.
        /// </summary>
        /// <param name="router">The router to scope the request to</param>
        /// <param name="segment">The segment which has been chunked of by the router from the request URI, if any</param>
        void Scope(IRouter router, string? segment = null);

        /// <summary>
        /// Registers the given content provider to be responsible
        /// to generate the content for the current request.
        /// </summary>
        /// <param name="contentProvider">The content provider to be registered</param>
        void RegisterContent(IContentProvider contentProvider);

    }

}
