using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add textareas to a container.
    /// </summary>
    public class TextareaCollection : ITextareaContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new textarea collection.
        /// </summary>
        /// <param name="d">The method used to add elements to the underlying collection</param>
        public TextareaCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region ITextareaContainer Members

        /// <summary>
        /// Add a new textarea.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="rows">The row count of the field</param>
        /// <param name="cols">The column count of the field</param>
        /// <returns>The created object</returns>
        public Textarea AddTextarea(string name, ushort rows, ushort cols)
        {
            Textarea area = new Textarea(name, rows, cols);
            _Delegate(area);
            return area;
        }

        /// <summary>
        /// Add a new textarea.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="rows">The row count of the field</param>
        /// <param name="cols">The column count of the field</param>
        /// <param name="value">The value of the textarea</param>
        /// <returns>The created object</returns>
        public Textarea AddTextarea(string name, ushort rows, ushort cols, string value)
        {
            Textarea area = new Textarea(name, rows, cols, value);
            _Delegate(area);
            return area;
        }

        #endregion

    }

}
