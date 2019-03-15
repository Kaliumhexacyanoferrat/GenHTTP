using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines methods which should be implemented by a container
    /// with option group elements.
    /// </summary>
    public interface IOptionGroupContainer
    {

        /// <summary>
        /// Add a option group.
        /// </summary>
        /// <param name="label">The label of the group</param>
        /// <returns>The created object</returns>
        OptionGroup AddOptionGroup(string label);

        /// <summary>
        /// Add a option group.
        /// </summary>
        /// <param name="label">The label of the group</param>
        /// <param name="isDisabled">Specifies, whether this option group is disabled or not</param>
        /// <returns>The created object</returns>
        OptionGroup AddOptionGroup(string label, bool isDisabled);

    }

}
