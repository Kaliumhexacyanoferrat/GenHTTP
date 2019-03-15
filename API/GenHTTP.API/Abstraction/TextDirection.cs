using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// Direction for weak/neutral text.
    /// </summary>
    public enum TextDirection
    {
        /// <summary>
        /// Write the text from the right to the left.
        /// </summary>
        RightToLeft,
        /// <summary>
        /// Write the text from the left to the right.
        /// </summary>
        LeftToRight,
        /// <summary>
        /// Do not specify this value for the element
        /// or <see cref="Document" />.
        /// </summary>
        Unspecified
    }

}
