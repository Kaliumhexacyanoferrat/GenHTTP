using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add select controls to a container.
    /// </summary>
    public class SelectCollection : ISelectContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new select collection.
        /// </summary>
        /// <param name="d">The method used to add elements to the underlying container</param>
        public SelectCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region ISelectContainer Members

        /// <summary>
        /// Add a new selection list.
        /// </summary>
        /// <param name="name">The name of the list</param>
        /// <returns>The created object</returns>
        public Select AddSelect(string name)
        {
            Select select = new Select(name);
            _Delegate(select);
            return select;
        }

        /// <summary>
        /// Add a new selection list.
        /// </summary>
        /// <param name="name">The name of the list</param>
        /// <param name="size">The size of the list</param>
        /// <returns>The created object</returns>
        public Select AddSelect(string name, ushort size)
        {
            Select select = new Select(name, size);
            _Delegate(select);
            return select;
        }

        /// <summary>
        /// Add a new selection list.
        /// </summary>
        /// <param name="name">The name of the list</param>
        /// <param name="size">The size of the list</param>
        /// <param name="multiple">Specifies, whether the user can select multiple entries</param>
        /// <returns>The created object</returns>
        public Select AddSelect(string name, ushort size, bool multiple)
        {
            Select select = new Select(name, size, multiple);
            _Delegate(select);
            return select;
        }

        #endregion

    }

}
