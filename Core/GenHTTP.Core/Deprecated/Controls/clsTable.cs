using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenHTTP.Style;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// A table in HTML.
  /// </summary>
  public class Table : HtmlElement, IItem {
    private string _ID;
    private int? _CellSpacing = 0, _CellPadding = 0;
    private Align _Align = Align.None;
    private ItemCollection _Children;

    /// <summary>
    /// Create a new table.
    /// </summary>
    public Table() {
      _Children = new ItemCollection();
    }

    /// <summary>
    /// The alignment of the table.
    /// </summary>
    public new Align Align {
      get { return _Align; }
      set { _Align = value; }
    }

    /// <summary>
    /// The distance between cells.
    /// </summary>
    public int? CellSpacing {
      get { return _CellSpacing; }
      set { _CellSpacing = value; }
    }

    /// <summary>
    /// The distance in cells from the border.
    /// </summary>
    public int? CellPadding {
      get { return _CellPadding; }
      set { _CellPadding = value; }
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Table" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Table; }
    }

    /// <summary>
    /// Retrieve all children of this element.
    /// </summary>
    public ItemCollection Children {
      get { return _Children; }
    }

    #region Children handling

    /// <summary>
    /// Retrieve an enumerator to iterate over the children of this element.
    /// </summary>
    /// <returns>The requested enumerator</returns>
    public IEnumerator<IItem> GetEnumerator() {
      return _Children.GetEnumerator();
    }

    /// <summary>
    /// Retrieve a child element by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem this[string id] {
      get { return _Children[id]; }
    }

    /// <summary>
    /// Retrieve a child element by its position in the item collection.
    /// </summary>
    /// <param name="pos">The position of the item to retrieve</param>
    /// <returns>The requested element</returns>
    public IItem this[int pos] {
      get { return _Children[pos]; }
    }

    #endregion

    /// <summary>
    /// Get or set the ID of this element.
    /// </summary>
    public string ID {
      get {
        return _ID;
      }
      set {
        _ID = value;
      }
    }

    /// <summary>
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string id = "";
      if (_ID != null) id = " id=\"" + _ID + "\"";
      string align = "";
      switch (_Align) {
        case Align.Center:
          align = " align=\"center\"";
          break;
        case Align.Right:
          align = " align=\"right\"";
          break;
        case Align.Left:
          align = " align=\"left\"";
          break;
      }
      string cellpadding = "";
      if (_CellPadding.HasValue) cellpadding = " cellpadding=\"" + _CellPadding.Value + "px\"";
      string cellspacing = "";
      if (_CellSpacing.HasValue) cellspacing = " cellspacing=\"" + _CellSpacing.Value + "px\"";
      string css = "";
      if (CssString != "") css = " style=\"" + CssString + "\"";
      string classes = "";
      if (Classes.ToCss() != "") classes = " class=\"" + Classes.ToCss() + "\"";
      builder.Append("<table" + id + align + cellpadding + cellspacing + css + classes + JsEvents + ">" + Environment.NewLine);
      foreach (IItem child in _Children) {
        child.SerializeContent(builder, style);
      }
      builder.Append("</table>" + Environment.NewLine);
    }

    /// <summary>
    /// Search recursively for a child with the given ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element, or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return _Children[id];
    }

  }

}
