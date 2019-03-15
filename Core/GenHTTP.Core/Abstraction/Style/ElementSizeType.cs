using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Style
{

    /// <summary>
    /// Defines the different sizing types.
    /// </summary>
    public enum ElementSizeType
    {
        /// <summary>
        /// The 'font-size' of the relevant font.
        /// </summary>
        Em,
        /// <summary>
        /// The 'x-height' of the relevant font.
        /// </summary>
        Ex,
        /// <summary>
        /// Pixels, relative to the viewing device.
        /// </summary>
        Px,
        /// <summary>
        /// Inches.
        /// </summary>
        In,
        /// <summary>
        /// Centimeters.
        /// </summary>
        Cm,
        /// <summary>
        /// Millimeters.
        /// </summary>
        Mm,
        /// <summary>
        /// Points.
        /// </summary>
        Pt,
        /// <summary>
        /// Picas.
        /// </summary>
        Pc,
        /// <summary>
        /// Percent. Used for container elements.
        /// </summary>
        Percent,
        /// <summary>
        /// No size.
        /// </summary>
        None
    }

}
