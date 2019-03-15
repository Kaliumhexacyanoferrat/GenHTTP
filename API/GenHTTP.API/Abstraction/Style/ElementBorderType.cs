using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Style
{

    /// <summary>
    /// Specifies the type of a border.
    /// </summary>
    public enum ElementBorderType
    {
        /// <summary>
        /// In border conflict resolution for table elements.
        /// </summary>
        Hidden,
        /// <summary>
        /// Specifies a dotted border.
        /// </summary>
        Dotted,
        /// <summary>
        /// Specifies a dashed border.
        /// </summary>
        Dashed,
        /// <summary>
        /// Specifies a solid border.
        /// </summary>
        Solid,
        /// <summary>
        /// Specifies a double border.
        /// </summary>
        Double,
        /// <summary>
        /// Specifies a 3D grooved border.
        /// </summary>
        Groove,
        /// <summary>
        /// Specifies a 3D ridged border.
        /// </summary>
        Ridge,
        /// <summary>
        /// Specifies a 3D inset border.
        /// </summary>
        Inset,
        /// <summary>
        /// Specifies a 3D outset border.
        /// </summary>
        Outset
    }

}
