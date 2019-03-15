using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// Represents a table cell.
    /// </summary>
    public class TableCell : StyledContainerElement
    {
        private byte _Colspan;
        private byte _Rowspan;
        private string _Headers;
        private string _Axis;
        private bool _IsHead;
        private string _Abbr;
        private TableCellScope _Scope = TableCellScope.Unspecified;

        #region Constructors

        /// <summary>
        /// Create a new, empty table cell.
        /// </summary>
        public TableCell()
        {

        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="isHead">Specifies, whether this cell is a table header ('th') cell or not</param>
        public TableCell(bool isHead)
        {
            _IsHead = isHead;
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="content">The content of the cell</param>
        public TableCell(string content)
        {
            AddText(content);
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="colspan">The colspan of the cell</param>
        public TableCell(byte colspan)
        {
            _Colspan = colspan;
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="content">The content of the cell</param>
        /// <param name="colspan">The colspan of the cell</param>
        public TableCell(string content, byte colspan)
        {
            AddText(content);
            _Colspan = colspan;
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="content">The content of the table cell</param>
        /// <param name="colspan">The colspan of the cell</param>
        /// <param name="rowspan">The rowspan of the cell</param>
        public TableCell(string content, byte colspan, byte rowspan)
        {
            AddText(content);
            _Rowspan = rowspan;
            _Colspan = colspan;
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="element">The content of the table cell</param>
        public TableCell(Element element)
        {
            Add(element);
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="element">The content of the table cell</param>
        /// <param name="colspan">The colspan of the table cell</param>
        public TableCell(Element element, byte colspan)
        {
            Add(element);
            _Colspan = colspan;
        }

        /// <summary>
        /// Create a new table cell.
        /// </summary>
        /// <param name="element">The content of the table cell</param>
        /// <param name="colspan">The colspan of the cell</param>
        /// <param name="rowspan">the rowspan of the cell</param>
        public TableCell(Element element, byte colspan, byte rowspan)
        {
            Add(element);
            _Rowspan = rowspan;
            _Colspan = colspan;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// How many table cells should get connected together?
        /// </summary>
        public byte Colspan
        {
            get { return _Colspan; }
            set { _Colspan = value; }
        }

        /// <summary>
        /// How many table rows should get connected together?
        /// </summary>
        public byte Rowspan
        {
            get { return _Rowspan; }
            set { _Rowspan = value; }
        }

        /// <summary>
        /// A list with header names, seperated by a single space ( ).
        /// </summary>
        public string Headers
        {
            get { return _Headers; }
            set { _Headers = value; }
        }

        /// <summary>
        /// The category of the cell.
        /// </summary>
        public string Axis
        {
            get { return _Axis; }
            set { _Axis = value; }
        }

        /// <summary>
        /// The scope of this table (header) cell.
        /// </summary>
        public TableCellScope Scope
        {
            get { return _Scope; }
            set { _Scope = value; }
        }

        /// <summary>
        /// Short description of the cell's content.
        /// </summary>
        public string Abbr
        {
            get { return _Abbr; }
            set { _Abbr = value; }
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
            if (_IsHead) b.Append("    <th"); else b.Append("    <td");
            if (_Scope != TableCellScope.Unspecified) b.Append(" scope=\"" + _Scope.ToString().ToLower() + "\"");
            if (_Headers != null && _Headers.Length > 0) b.Append(" headers=\"" + _Headers + "\"");
            if (_Abbr != null && _Abbr.Length > 0) b.Append(" abbr=\"" + _Abbr + "\"");
            if (_Colspan > 0) b.Append(" colspan=\"" + _Colspan + "\"");
            if (_Rowspan > 0) b.Append(" rowspan=\"" + _Rowspan + "\"");
            b.Append(ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">");
            SerializeChildren(b, type);
            if (_IsHead) b.Append("</th>"); else b.Append("</td>\r\n");
        }

        #endregion

    }

}
