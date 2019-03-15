using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines methods which should be implemented by a container
    /// with select elements.
    /// </summary>
    public interface ISelectContainer
    {

        /// <summary>
        /// Add a new selection list.
        /// </summary>
        /// <param name="name">The name of the list</param>
        /// <returns>The created object</returns>
        Select AddSelect(string name);

        /// <summary>
        /// Add a new selection list.
        /// </summary>
        /// <param name="name">The name of the list</param>
        /// <param name="size">The size of the list</param>
        /// <returns>The created object</returns>
        Select AddSelect(string name, ushort size);

        /// <summary>
        /// Add a new selection list.
        /// </summary>
        /// <param name="name">The name of the list</param>
        /// <param name="size">The size of the list</param>
        /// <param name="multiple">Specifies, whether the user can select multiple entries</param>
        /// <returns>The created object</returns>
        Select AddSelect(string name, ushort size, bool multiple);

    }

}
