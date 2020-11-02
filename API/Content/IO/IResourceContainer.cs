﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GenHTTP.Api.Content.IO
{

    /// <summary>
    /// Provides a single hierarchy level in a structure
    /// provided by a resource tree.
    /// </summary>
    public interface IResourceContainer
    {

        /// <summary>
        /// Tries to fetch the child node with the given name.
        /// </summary>
        /// <param name="name">The name of the node to be fetched</param>
        /// <param name="node">The node fetched from the container</param>
        /// <returns>true, if the node could be found</returns>
        bool TryGetNode(string name, [MaybeNullWhen(returnValue: false)] out IResourceNode node);

        /// <summary>
        /// Returns the child nodes provided by this container.
        /// </summary>
        /// <returns>The child nodes provided by this container</returns>
        IEnumerable<IResourceNode> GetNodes();

        /// <summary>
        /// Tries to fetch the resource with the given name.
        /// </summary>
        /// <param name="name">The name of the resource to be fetched</param>
        /// <param name="node">The resource fetched from the container</param>
        /// <returns>true, if the resource could be found</returns>
        bool TryGetResource(string name, [MaybeNullWhen(returnValue: false)] out IResource node);

        /// <summary>
        /// Returns the resources provided by this container.
        /// </summary>
        /// <returns>The resources provided by this container</returns>
        IEnumerable<IResource> GetResources();

    }

}
