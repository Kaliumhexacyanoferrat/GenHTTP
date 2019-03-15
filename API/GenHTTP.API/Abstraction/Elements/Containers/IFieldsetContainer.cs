using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// This interface should be implemented by container
    /// with fieldset elements.
    /// </summary>
    public interface IFieldsetContainer
    {

        /// <summary>
        /// Add a new fieldset.
        /// </summary>
        /// <param name="caption">The caption of the set</param>
        /// <returns>The created object</returns>
        Fieldset AddFieldset(string caption);

    }

}
