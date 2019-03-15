using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// Horizontal alignment of text in a control.
  /// </summary>
  public enum Align {
    /// <summary>
    /// Use standard.
    /// </summary>
    None,
    /// <summary>
    /// Align the text left.
    /// </summary>
    Left,
    /// <summary>
    /// Align the text right.
    /// </summary>
    Right,
    /// <summary>
    /// Align the text in the middle.
    /// </summary>
    Center
  }

  /// <summary>
  /// Vertical alignment of a text in a control.
  /// </summary>
  public enum VAlign {
    /// <summary>
    /// Use standard.
    /// </summary>
    None,
    /// <summary>
    /// Align the text on top.
    /// </summary>
    Top,
    /// <summary>
    /// Align the text in the middle.
    /// </summary>
    Middle,
    /// <summary>
    /// Align the text at bottom.
    /// </summary>
    Bottom
  }

  /// <summary>
  /// CSS property float, used by DIVs, for example.
  /// </summary>
  public enum Float {
    /// <summary>
    /// float:left
    /// </summary>
    Left,
    /// <summary>
    /// float:right
    /// </summary>
    Right,
    /// <summary>
    /// Use standard.
    /// </summary>
    None
  }

  /// <summary>
  /// The div-control representation.
  /// </summary>
  public class Div : HtmlElement, IItem {
    private ItemCollection _Children;
    private Float _Float = Float.None;
    private string _ID;

    /// <summary>
    /// Create a new div element.
    /// </summary>
    public Div() {
      _Children = new ItemCollection();
    }

    /// <summary>
    /// Create a new div element.
    /// </summary>
    /// <param name="id">The id of this element</param>
    public Div(string id) : this() {
      _ID = id;
    }

    /// <summary>
    /// The type of this element.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Div; }
    }

    /// <summary>
    /// The id of this element.
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
    /// Set or get the floating of this element.
    /// </summary>
    public Float Float {
      get { return _Float; }
      set { _Float = value; }
    }

    /// <summary>
    /// Retrieve the children of this element.
    /// </summary>
    public ItemCollection Children {
      get {
        return _Children;
      }
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
    /// Retrieve (X)HTML output.
    /// </summary>
    /// <param name="builder">The string builder to append to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string nl = Environment.NewLine;
      string css = "", classes = "", f = "", id = "";
      if (_ID != null && _ID != "") id = " id=\"" + _ID + "\"";
      if (Classes.ToCss() != "") classes = " class=\"" + Classes.ToCss() + "\"";
      switch (_Float) {
        case Float.Left: f = "float: left; "; break;
        case Float.Right: f = "float: right; "; break;
      }
      if (CssString != "") css = " style=\"" + CssString + f + "\"";
      builder.Append(nl + "<div" + id + css + classes + JsEvents + ">" + nl);
      foreach (IItem child in _Children) {
        child.SerializeContent(builder, style);
      }
      builder.Append(nl + "</div>" + nl);
    }

    /// <summary>
    /// Retrieve an element with a given id recursively
    /// </summary>
    /// <param name="id">The id of the element to search for</param>
    /// <returns>The element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return _Children[id];
    }

  }

}
