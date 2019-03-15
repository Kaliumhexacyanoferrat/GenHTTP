using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add option group elements to a container.
    /// </summary>
    public class OptionGroupCollection : IOptionGroupContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new option group collection.
        /// </summary>
        /// <param name="d">The method used to add group elements to the container</param>
        public OptionGroupCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region IOptionGroupContainer Members

        /// <summary>
        /// Add a option group.
        /// </summary>
        /// <param name="label">The label of the group</param>
        /// <returns>The created object</returns>
        public OptionGroup AddOptionGroup(string label)
        {
            OptionGroup group = new OptionGroup(label);
            _Delegate(group);
            return group;
        }

        /// <summary>
        /// Add a option group.
        /// </summary>
        /// <param name="label">The label of the group</param>
        /// <param name="isDisabled">Specifies, whether this option group is disabled or not</param>
        /// <returns>The created object</returns>
        public OptionGroup AddOptionGroup(string label, bool isDisabled)
        {
            OptionGroup group = new OptionGroup(label, isDisabled);
            _Delegate(group);
            return group;
        }

        #endregion

    }

}
