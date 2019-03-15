using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines the methods which a container with
    /// table elements must provide.
    /// </summary>
    public interface ITableContainer
    {

        /// <summary>
        /// Add a new table.
        /// </summary>
        /// <returns>The created table</returns>
        Table AddTable();

    }

}
