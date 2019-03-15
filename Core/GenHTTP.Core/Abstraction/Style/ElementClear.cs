using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Style
{

    /// <summary>
    /// Clears the flow of an element.
    /// </summary>
    public enum ElementClear
    {
        /// <summary>
        /// Default.
        /// </summary>
        None,
        /// <summary>
        /// Left flow.
        /// </summary>
        Left,
        /// <summary>
        /// Right flow.
        /// </summary>
        Right,
        /// <summary>
        /// Flow.
        /// </summary>
        Both,
        /// <summary>
        /// Not specified.
        /// </summary>
        Unspecified
    }

}
