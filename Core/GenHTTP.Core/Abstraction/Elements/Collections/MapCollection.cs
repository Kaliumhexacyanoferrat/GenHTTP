using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add a map element to an <see cref="IMapContainer" />.
    /// </summary>
    public class MapCollection : IMapContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new map collection.
        /// </summary>
        /// <param name="d">The method used to add the map elements to the container</param>
        public MapCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region IMapContainer Members

        /// <summary>
        /// Add a new map.
        /// </summary>
        /// <param name="name">The name of the map</param>
        /// <returns>The created object</returns>
        public Map AddMap(string name)
        {
            Map map = new Map(name);
            _Delegate(map);
            return map;
        }

        #endregion

    }

}
