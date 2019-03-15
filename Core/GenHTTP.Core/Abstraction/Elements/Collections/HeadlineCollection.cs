using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add headlines to a container.
    /// </summary>
    public class HeadlineCollection : IHeadlineContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new headline collection.
        /// </summary>
        /// <param name="d">The method used to add an element to the underlying container</param>
        public HeadlineCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region IHeadlineContainer Members

        /// <summary>
        /// Add a new headline.
        /// </summary>
        /// <param name="value">The value of the headline</param>
        /// <returns>The created object</returns>
        public Headline AddHeadline(string value)
        {
            Headline line = new Headline(value);
            _Delegate(line);
            return line;
        }

        /// <summary>
        /// Add a new headline.
        /// </summary>
        /// <param name="value">The value of the headline</param>
        /// <param name="size">The size of the headline (from 1 to 6)</param>
        /// <returns>The created object</returns>
        public Headline AddHeadline(string value, byte size)
        {
            Headline line = new Headline(value, size);
            _Delegate(line);
            return line;
        }

        #endregion

    }

}
