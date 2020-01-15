using System;
using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Routes a request from the client to the content provider
    /// responsible for generating the HTTP response.
    /// </summary>
    public interface IRouter
    {

        /// <summary>
        /// The parent of this router within the router chain.
        /// </summary>
        /// <remarks>
        /// This property will automatically be set by the parent.
        /// </remarks>
        IRouter Parent { get; set; }

        /// <summary>
        /// Called to determine the content provider to generate
        /// a response for the given request.
        /// </summary>
        /// <param name="current">The routing content to be adjusted by the router</param>
        void HandleContext(IEditableRoutingContext current);

        /// <summary>
        /// Fetches the template to be used for responses generated 
        /// by this router.
        /// </summary>
        /// <returns>The template renderer to be used</returns>
        /// <remarks>
        /// If a router cannot tell the template to be used, the 
        /// renderer of the parent should be returned.
        /// </remarks>
        IRenderer<TemplateModel> GetRenderer();

        /// <summary>
        /// Fetches the error handler to be used for responses generated
        /// by this router.
        /// </summary>
        /// <param name="request">The request an error should be generated for</param>
        /// <param name="responseType">The kind of response to be generated</param>
        /// <param name="cause">The error which has occurred (if any)</param>
        /// <returns>The content provider which will generate the error response</returns>
        IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause);

        IEnumerable<ContentElement> GetContent(IRequest request, string basePath);

        /// <summary>
        /// Determines the relative path to be used to navigate to the
        /// requested path.
        /// </summary>
        /// <param name="path">The path to generate a route for</param>
        /// <param name="currentDepth">The number of routing segments alread traversed before this instance was called</param>
        /// <returns>The relative path to navigate to the requested path</returns>
        /// <remarks>
        /// If a router is not responsible for providing the route to the
        /// requested resource, the call should be chained to the parent router
        /// with an incremented depth. Note, that the depth should only be increased
        /// if if the router works on segments (such as /segment/ within an URL). For
        /// example, the virtual hosts router does not use the URL for routing, so
        /// it should not increment the depth.
        /// </remarks>
        string? Route(string path, int currentDepth);

    }

}
