using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// This interface defines methodes which must be provided by
    /// containers with table section elements.
    /// </summary>
    public interface ITableSectionContainer
    {

        /// <summary>
        /// Add a new table section.
        /// </summary>
        /// <param name="type">The type of the table section</param>
        /// <returns>The created object</returns>
        TableSection AddTableSection(TableSectionType type);

    }

}
