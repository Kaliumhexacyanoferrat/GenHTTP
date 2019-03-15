using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Style;

namespace GenHTTP
{

    /// <summary>
    /// Stores project specific colors.
    /// </summary>
    public class ProjectColorCollection
    {
        private Dictionary<string, ElementColor> _Colors;

        #region Constructors

        /// <summary>
        /// Create a new project color collection.
        /// </summary>
        public ProjectColorCollection()
        {
            _Colors = new Dictionary<string, ElementColor>();
        }

        #endregion

        #region Collection handling

        /// <summary>
        /// Add a color to the collection.
        /// </summary>
        /// <param name="name">The name of the color</param>
        /// <param name="color">The color to add</param>
        public void Add(string name, ElementColor color)
        {
            if (_Colors.ContainsKey(name)) _Colors[name] = color;
            else _Colors.Add(name, color);
        }

        /// <summary>
        /// Remove a color from this collection.
        /// </summary>
        /// <param name="name">The name of the color to remove</param>
        public void Remove(string name)
        {
            if (_Colors.ContainsKey(name)) _Colors.Remove(name);
        }

        /// <summary>
        /// The number of colors in this collection.
        /// </summary>
        public int Count
        {
            get { return _Colors.Count; }
        }

        /// <summary>
        /// Retrieve a color by its name.
        /// </summary>
        /// <param name="name">The name of the color</param>
        /// <returns>The requested color or null, if it could no be found</returns>
        public ElementColor this[string name]
        {
            get
            {
                if (_Colors.ContainsKey(name)) return _Colors[name];
                return null;
            }
        }

        /// <summary>
        /// Retrieve an enumerator to iterate over this collection.
        /// </summary>
        /// <returns>The requested enumerator</returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _Colors.Keys.GetEnumerator();
        }

        #endregion


    }

}
