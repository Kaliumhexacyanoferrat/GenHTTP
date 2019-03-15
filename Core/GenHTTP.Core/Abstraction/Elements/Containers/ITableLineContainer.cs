using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// This interface describes, which methods a container for
    /// table lines must provide.
    /// </summary>
    public interface ITableLineContainer
    {

        /// <summary>
        /// Add a new, empty table line.
        /// </summary>
        /// <returns>The created object</returns>
        TableLine AddTableLine();

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        TableLine AddTableLine(string[] cells);

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        TableLine AddTableLine(IEnumerable<string> cells);

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        TableLine AddTableLine(Element[] cells);

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        TableLine AddTableLine(IEnumerable<Element> cells);

    }

}
