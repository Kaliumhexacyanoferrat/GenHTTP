using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// The routing context of a request, accessible
    /// by content providers when generating content.
    /// </summary>
    public interface IRoutingContext
    {

        /// <summary>
        /// The router responsible for this request.
        /// </summary>
        IRouter Router { get; }

        /// <summary>
        /// The request to be handled.
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// The remaining part of the request URI after
        /// the router chain has processed the request.
        /// </summary>
        string ScopedPath { get; }

        /// <summary>
        /// The content provider responsible to handle the
        /// current request.
        /// </summary>
        IContentProvider? ContentProvider { get; }

        /// <summary>
        /// Determine the relative path to be used to access the
        /// given resource.
        /// </summary>
        /// <param name="route">The resource to be accessed</param>
        /// <returns>The relative path to the resource, if it can be determined</returns>
        string? Route(string route);

    }

}
