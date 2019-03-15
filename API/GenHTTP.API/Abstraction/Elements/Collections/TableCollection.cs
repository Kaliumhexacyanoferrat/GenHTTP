using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add tables to a container implementing
    /// the <see cref="ITableContainer" /> interface.
    /// </summary>
    public class TableCollection : ITableContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new table collection.
        /// </summary>
        /// <param name="d">The method used to add an element to the underlying container</param>
        public TableCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region ITableContainer Members

        /// <summary>
        /// Add a new table.
        /// </summary>
        /// <returns>The created table</returns>
        public Table AddTable()
        {
            Table table = new Table();
            _Delegate(table);
            return table;
        }

        #endregion

    }

}
