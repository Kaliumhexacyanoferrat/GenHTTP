using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Style;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// Defines a table column.
    /// </summary>
    public class TableColumn : Element
    {
        private ElementSize _Width;
        private byte _Span;

        #region Constructors

        /// <summary>
        /// Create a new table column.
        /// </summary>
        /// <param name="width">The width of the table column</param>
        public TableColumn(ElementSize width)
        {
            _Width = width;
        }

        /// <summary>
        /// Create a new table column.
        /// </summary>
        /// <param name="width">The width of the table column</param>
        public TableColumn(ushort width)
        {
            _Width = new ElementSize(width);
        }

        /// <summary>
        /// Create a new table column.
        /// </summary>
        /// <param name="width">The width of the table column</param>
        /// <param name="span">Specifiy, for how many columns this width applies (including this)</param>
        public TableColumn(ElementSize width, byte span)
        {
            _Width = width;
            _Span = span;
        }

        /// <summary>
        /// Create a new table column.
        /// </summary>
        /// <param name="width">The width of the table column</param>
        /// <param name="span">Specifiy, for how many columns this width applies (including this)</param>
        public TableColumn(ushort width, byte span)
        {
            _Width = new ElementSize(width);
            _Span = span;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The width of this table column.
        /// </summary>
        public ElementSize Width
        {
            get { return _Width; }
            set
            {
                if (value == null) throw new ArgumentNullException("The width of a table column is a required attribute");
                _Width = value;
            }
        }

        /// <summary>
        /// Specifies, for how many columns this width applies (including this column).
        /// </summary>
        public byte Span
        {
            get { return _Span; }
            set { _Span = value; }
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
            b.Append("    <col width=\"" + _Width.ToCss() + "\"");
            if (_Span > 0) b.Append(" span=\"" + _Span + "\"");
            if (IsXHtml) b.Append(" />"); else b.Append(">");
            b.Append("\r\n");
        }

        #endregion

    }

}
