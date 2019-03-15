using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Style
{

    /// <summary>
    /// Specifies the decoration of text.
    /// </summary>
    public enum ElementTextDecoration
    {
        /// <summary>
        /// No text decoration.
        /// </summary>
        None,
        /// <summary>
        /// Underlined text.
        /// </summary>
        Underline,
        /// <summary>
        /// Overlined text.
        /// </summary>
        Overline,
        /// <summary>
        /// Lined trough text.
        /// </summary>
        LineThrough,
        /// <summary>
        /// Blinking text.
        /// </summary>
        Blink,
        /// <summary>
        /// Not specified.
        /// </summary>
        Unspecified
    }

}
