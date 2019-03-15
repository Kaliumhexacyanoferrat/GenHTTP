using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// Allows you to add links to a container.
    /// </summary>
    public interface ILinkContainer
    {

        /// <summary>
        /// Add an empty link.
        /// </summary>
        /// <returns>The created object</returns>
        Link AddLink();

        /// <summary>
        /// Add a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <returns>The created object</returns>
        Link AddLink(string url);

        /// <summary>
        /// Add a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <param name="linkText">The link text</param>
        /// <returns>The created object</returns>
        Link AddLink(string url, string linkText);

        /// <summary>
        /// Add a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <param name="element">The link element</param>
        /// <returns>The created object</returns>
        Link AddLink(string url, Element element);

    }

}
