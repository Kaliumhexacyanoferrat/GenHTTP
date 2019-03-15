using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Style;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// Allows you to format text.
    /// </summary>
    public class Span : StyledContainerElement
    {

        #region Constructors

        /// <summary>
        /// Create a new, empty span.
        /// </summary>
        public Span()
        {

        }

        /// <summary>
        /// Create a new span.
        /// </summary>
        /// <param name="text">The content of the span</param>
        /// <param name="decoration">The text decoration to use</param>
        public Span(string text, ElementTextDecoration decoration)
        {
            AddText(text);
            TextDecoration = decoration;
        }

        /// <summary>
        /// Create a new span.
        /// </summary>
        /// <param name="text">The content of the span</param>
        /// <param name="weight">The font weight to use</param>
        public Span(string text, ElementFontWeight weight)
        {
            AddText(text);
            FontWeight = weight;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            b.Append("<span" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">");
            SerializeChildren(b, type);
            b.Append("</span>");
        }

        #endregion

    }

}
