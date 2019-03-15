using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// A line to seperate content.
    /// </summary>
    public class Line : StyledElement
    {

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            b.Append("\r\n<hr" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
            b.Append((IsXHtml) ? " />" : ">\r\n");
        }

    }

}
