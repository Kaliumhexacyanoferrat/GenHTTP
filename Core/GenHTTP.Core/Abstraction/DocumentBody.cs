using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements;
using GenHTTP.Abstraction.Style;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// Describes the body of a <see cref="Document" />.
    /// </summary>
    public class DocumentBody : StyledContainerElement
    {

        #region Serialization

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            b.Append("<body" + ToXHtml(IsXHtml) + ToClassString() + ToCss(true) + ">");
            SerializeChildren(b, type);
            b.Append("</body>\r\n\r\n");
        }

        #endregion

    }

}
