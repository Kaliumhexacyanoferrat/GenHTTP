using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenHTTP.Style;

namespace GenHTTP.Controls {

  /// <summary>
  /// Represents a cell in a HTML table.
  /// </summary>
  public class Td : HtmlElement, IItem {
    private string _ID;
    private int? _Colspan;
    private int? _Rowspan;
    private ItemCollection _Children;
    private VAlign _VAlign = VAlign.None;

    /// <summary>
    /// Create a new HTML table cell.
    /// </summary>
    public Td() {
      _Children = new ItemCollection();
    }

    /// <summary>
    /// Create a new HTML table cell.
    /// </summary>
    /// <param name="text">The content of the table cell</param>
    public Td(string text) : this() {
      _Children.Insert(new Text(text));
    }

    /// <summary>
    /// Retrieve all childs of this cell.
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
    /// The type of this element. Should be <see cref="ItemType.Td" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Td; }
    }

    /// <summary>
    /// The vertical alignment of this cell.
    /// </summary>
    public VAlign VAlign {
      get { return _VAlign; }
      set { _VAlign = value; }
    }

    /// <summary>
    /// Get or set the ID of this cell.
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
    /// The colspan of this cell (to merge two cells).
    /// </summary>
    public int? Colspan {
      get { return _Colspan; }
      set { _Colspan = value; }
    }

    /// <summary>
    /// The rowspawn of this cell (to merge two cells).
    /// </summary>
    public int? Rowspan {
      get { return _Rowspan; }
      set { _Rowspan = value; }
    }

    /// <summary>
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string id = "";
      if (_ID != null) id = " id=\"" + _ID + "\"";
      string colspan = "";
      if (_Colspan.HasValue) colspan = " colspan=\"" + _Colspan + "\"";
      string rowspan = "";
      if (_Rowspan.HasValue) rowspan = " rowspan=\"" + _Rowspan + "\"";
      string css = "";
      if (CssString != "") css = " style=\"" + CssString + "\"";
      string classes = "";
      if (Classes.ToCss() != "") classes = " class=\"" + Classes.ToCss() + "\"";
      string valign = "";
      switch (_VAlign) {
        case VAlign.Bottom: valign = " valign=\"bottom\""; break;
        case VAlign.Top: valign = " valign=\"top\""; break;
        case VAlign.Middle: valign = " valign=\"middle\""; break;
      }
      builder.Append("    <td" + id + valign + css + classes + colspan + rowspan + JsEvents + ">");
      foreach (IItem child in _Children) {
        child.SerializeContent(builder, style);
      }
      builder.Append("</td>" + Environment.NewLine);
    }
  
    /// <summary>
    /// Search recursively for a element by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return _Children[id];
    }

  }

}
