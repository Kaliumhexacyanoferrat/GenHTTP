using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO.Tracking;

namespace GenHTTP.Modules.IO
{

    public static class ChangeTracking
    {

        /// <summary>
        /// Wraps the given resource into a change tracker that can
        /// tell, whether the content of the resource has changed
        /// since it's content was read the last time.
        /// </summary>
        /// <param name="resource">The resource to be tracked</param>
        /// <returns>The given resource with change tracking attached</returns>
        /// <remarks>
        /// Use this mechanism for functionality that needs to
        /// perform computation heavy work such as parsing
        /// or interpreting the content of the resource.
        /// </remarks>
        public static ChangeTrackingResource Track(this IResource resource) => new ChangeTrackingResource(resource);

        /// <summary>
        /// Builds the resource and wraps it into a change tracker that can
        /// tell, whether the content of the resource has changed
        /// since it's content was read the last time.    
        /// </summary>
        /// <param name="resourceBuilder">The builder of the resource to be tracked</param>
        /// <returns>The newly created resource with change tracking attached</returns>
        /// <remarks>
        /// Use this mechanism for functionality that needs to
        /// perform computation heavy work such as parsing
        /// or interpreting the content of the resource.
        /// </remarks>
        public static ChangeTrackingResource BuildWithTracking(this IBuilder<IResource> resourceBuilder) => new ChangeTrackingResource(resourceBuilder.Build());

    }

}
