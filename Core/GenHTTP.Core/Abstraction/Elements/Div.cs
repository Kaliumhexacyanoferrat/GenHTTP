using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// A container box.
    /// </summary>
    public class Div : StyledContainerElement
    {

        #region Serialization

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            b.Append("\r\n<div" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
            SerializeChildren(b, type);
            b.Append("\r\n</div>\r\n");
        }

        #endregion

    }

}
