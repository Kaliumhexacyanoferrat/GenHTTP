using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Style;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Provides methods which containers should implement if
    /// they contain span elements.
    /// </summary>
    public interface ISpanContainer
    {

        /// <summary>
        /// Add a new, empty span.
        /// </summary>
        /// <returns>The created object</returns>
        Span AddSpan();

        /// <summary>
        /// Add a new span.
        /// </summary>
        /// <param name="text">The content of the span</param>
        /// <param name="decoration">The text decoration to use</param>
        /// <returns>The created object</returns>
        Span AddSpan(string text, ElementTextDecoration decoration);

        /// <summary>
        /// Add a new span.
        /// </summary>
        /// <param name="text">The content of the span</param>
        /// <param name="fontWeight">The font weight to use</param>
        /// <returns>The created object</returns>
        Span AddSpan(string text, ElementFontWeight fontWeight);

    }

}
