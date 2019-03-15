using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines, which methods a container must provide,
    /// if it can contain map elements.
    /// </summary>
    public interface IMapContainer
    {

        /// <summary>
        /// Add a new map.
        /// </summary>
        /// <param name="name">The name of the map</param>
        /// <returns>The created object</returns>
        Map AddMap(string name);

    }

}
