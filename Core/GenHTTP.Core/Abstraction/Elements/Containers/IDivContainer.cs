using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines, how the user can add a 'div' element
    /// to a container.
    /// </summary>
    public interface IDivContainer
    {

        /// <summary>
        /// Add an empty div to the container.
        /// </summary>
        /// <returns>The created object</returns>
        Div AddDiv();

        /// <summary>
        /// Add an empty div to the container.
        /// </summary>
        /// <param name="id">The ID of the new Div</param>
        /// <returns>The created object</returns>
        Div AddDiv(string id);

        /// <summary>
        /// Add a div to the container.
        /// </summary>
        /// <param name="element">The content of the div box</param>
        /// <returns>The created object</returns>
        Div AddDiv(Element element);

    }

}
