using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;
using GenHTTP.Abstraction.Elements.Collections;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Represents a table line.
  /// </summary>
  public class TableLine : StyledElementWithChildren, ITableCellContainer {
    private TableCellCollection _Cells;

    #region Constructors

    /// <summary>
    /// Create a new table line.
    /// </summary>
    public TableLine() {
      _Cells = new TableCellCollection(Add);
    }

    /// <summary>
    /// Add a new table line.
    /// </summary>
    /// <param name="cells">Add additional table cells</param>
    public TableLine(string[] cells) : this() {
      foreach (string cell in cells) AddTableCell(cell);
    }

    /// <summary>
    /// Add a new table line.
    /// </summary>
    /// <param name="cells">Add additional table cells</param>
    public TableLine(IEnumerable<string> cells) : this() {
      foreach (string cell in cells) AddTableCell(cell);
    }

    /// <summary>
    /// Add a new table line.
    /// </summary>
    /// <param name="cells">Add additional table cells</param>
    public TableLine(Element[] cells) : this() {
      foreach (Element cell in cells) AddTableCell(cell);
    }

    /// <summary>
    /// Add a new table line.
    /// </summary>
    /// <param name="cells">Add additional table cells</param>
    public TableLine(IEnumerable<Element> cells) : this() {
      foreach (Element cell in cells) AddTableCell(cell);
    }

    #endregion

    #region ITableCellContainer Members

    /// <summary>
    /// Add a new, empty table cell.
    /// </summary>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell() {
      return _Cells.AddTableCell();
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="isHead">Specifiy, whether this cell is a header cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(bool isHead) {
      return _Cells.AddTableCell(isHead);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(string content) {
      return _Cells.AddTableCell(content);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(byte colspan) {
      return _Cells.AddTableCell(colspan);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(string content, byte colspan) {
      return _Cells.AddTableCell(content, colspan);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <param name="rowspan">The rowspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(string content, byte colspan, byte rowspan) {
      return _Cells.AddTableCell(content, colspan, rowspan);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(Element element) {
      return _Cells.AddTableCell(element);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(Element element, byte colspan) {
      return _Cells.AddTableCell(element, colspan);
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <param name="rowspan">The rowspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(Element element, byte colspan, byte rowspan) {
      return _Cells.AddTableCell(element, colspan, rowspan);
    }

    #endregion

    #region Element restriction

    /// <summary>
    /// Add an child element to this table line.
    /// </summary>
    /// <param name="element">The element to add</param>
    public override void Add(Element element) {
      if (!(element is TableCell)) throw new ArgumentException("You can only add table cells to a table line", "element");
      _Children.Add(element);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("  <tr" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
      SerializeChildren(b, type);
      b.Append("  </tr>\r\n");
    }

    #endregion

  }

}
