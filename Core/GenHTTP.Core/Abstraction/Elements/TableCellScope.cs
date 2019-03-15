using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// The scope of table header cells.
    /// </summary>
    public enum TableCellScope
    {
        /// <summary>
        /// The TD provides header information for the rest of the row.
        /// </summary>
        Row,
        /// <summary>
        /// The TD provides header information for the rest of the column.
        /// </summary>
        Col,
        /// <summary>
        /// The TD gives header information for the rest of the row group.
        /// </summary>
        RowGroup,
        /// <summary>
        /// The TD gives header information for the rest of the column group.
        /// </summary>
        ColGroup,
        /// <summary>
        /// Not specified.
        /// </summary>
        Unspecified
    }

}
