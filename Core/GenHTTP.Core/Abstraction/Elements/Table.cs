using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Collections;
using GenHTTP.Abstraction.Elements.Containers;
using GenHTTP.Abstraction.Style;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Defines a table.
  /// </summary>
  public class Table : StyledElementWithChildren, ITableSectionContainer, ITableLineContainer {
    private TableLineCollection _Lines;
    private TableSectionCollection _Sections;
    private NeutralElement _Caption;
    private TableColumnCollection _Columns;
    private bool _ContainsSections, _ContainsLines;
    private ushort _CellSpacing, _CellPadding;

    #region Constructors

    /// <summary>
    /// Create a new, empty table.
    /// </summary>
    public Table() {
      _Lines = new TableLineCollection(Add);
      _Sections = new TableSectionCollection(Add);
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The caption of the table.
    /// </summary>
    public NeutralElement Caption {
      get { return _Caption; }
      set { _Caption = value; }
    }

    /// <summary>
    /// The column definition of this table.
    /// </summary>
    public TableColumnCollection Columns {
      get { return _Columns; }
      set { _Columns = value; }
    }

    /// <summary>
    /// The space between cells in pixels.
    /// </summary>
    public ushort CellSpacing {
      get { return _CellSpacing; }
      set { _CellSpacing = value; }
    }

    /// <summary>
    /// The padding of cells in pixels.
    /// </summary>
    public ushort CellPadding {
      get { return _CellPadding; }
      set { _CellPadding = value; }
    }

    #endregion

    #region Element management

    /// <summary>
    /// Add a child element to this table.
    /// </summary>
    /// <param name="element">The element to add</param>
    /// <remarks>
    /// You can add either table sections or table lines to this
    /// table.
    /// </remarks>
    public override void Add(Element element) {
      if (!(element is TableSection) && !(element is TableLine)) throw new ArgumentException("A table can only contain table lines or sections", "element");
      if (Count == 0) { _ContainsSections = false; _ContainsLines = false; }
      if (element is TableSection && _ContainsLines) throw new ArgumentException("This table can only contain table lines");
      if (element is TableLine && _ContainsSections) throw new ArgumentException("This table can only contain table sections");
      if (element is TableLine) _ContainsLines = true;
      if (element is TableSection) _ContainsSections = true;
      _Children.Add(element);
    }

    #endregion

    #region ITableLineContainer Members

    /// <summary>
    /// Add a new, empty table line.
    /// </summary>
    /// <returns>The created object</returns>
    public TableLine AddTableLine() {
      return _Lines.AddTableLine();
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(string[] cells) {
      return _Lines.AddTableLine(cells);
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(IEnumerable<string> cells) {
      return _Lines.AddTableLine(cells);
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(Element[] cells) {
      return _Lines.AddTableLine(cells);
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(IEnumerable<Element> cells) {
      return _Lines.AddTableLine(cells);
    }

    #endregion

    #region ITableSectionContainer Members

    /// <summary>
    /// Add a new table section.
    /// </summary>
    /// <param name="type">The type of the table section</param>
    /// <returns>The created object</returns>
    public TableSection AddTableSection(TableSectionType type) {
      return _Sections.AddTableSection(type);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("\r\n<table" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + " cellspacing=\"" + CellSpacing + "\"");
      if (_CellPadding > 0) b.Append(" cellpadding=\"" + _CellPadding + "\"");
      b.Append("\">\r\n");
      if (_Caption != null) {
        b.Append("  <caption" + _Caption.ToClassString() + _Caption.ToXHtml(IsXHtml) + _Caption.ToCss() + ">");
        _Caption.Serialize(b, type);
        b.Append("</caption>\r\n");
      }
      if (_Columns != null) _Columns.Serialize(b, type);
      SerializeChildren(b, type);
      b.Append("</table>\r\n");
    }

    #endregion

  }

}
