using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenHTTP.Api.Content.IO
{

    /// <summary>
    /// Provides a single hierarchy level in a structure
    /// provided by a resource tree.
    /// </summary>
    public interface IResourceContainer
    {

        /// <summary>
        /// The point in time when the container was modified (if known).
        /// </summary>
        DateTime? Modified { get; }

        /// <summary>
        /// Tries to fetch the child node with the given name.
        /// </summary>
        /// <param name="name">The name of the node to be fetched</param>
        /// <returns>The node fetched from the container, if the node could be found</returns>
        ValueTask<IResourceNode?> TryGetNodeAsync(string name);

        /// <summary>
        /// Returns the child nodes provided by this container.
        /// </summary>
        /// <returns>The child nodes provided by this container</returns>
        IAsyncEnumerable<IResourceNode> GetNodes();

        /// <summary>
        /// Tries to fetch the resource with the given name.
        /// </summary>
        /// <param name="name">The name of the resource to be fetched</param>
        /// <param name="resource"></param>
        /// <returns>The resource fetched from the container, if the resource could be found</returns>
        ValueTask<IResource?> TryGetResourceAsync(string name);

        /// <summary>
        /// Returns the resources provided by this container.
        /// </summary>
        /// <returns>The resources provided by this container</returns>
        IAsyncEnumerable<IResource> GetResources();

    }

}
