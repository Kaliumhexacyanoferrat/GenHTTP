using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// Represents a span element in HTML.
  /// </summary>
  public class Span : HtmlElement, IItem {
    private string _ID;
    private ItemCollection _Children;

    /// <summary>
    /// Create a new span.
    /// </summary>
    public Span() {
      _Children = new ItemCollection();
    }

    /// <summary>
    /// Create a new span.
    /// </summary>
    /// <param name="id">The ID of the element</param>
    public Span(string id) : this() {
      _ID = id;
    }

    /// <summary>
    /// The type of the element. Should be <see cref="ItemType.Span" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Span;  }
    }

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
    /// Retrieve all sub items of this element.
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
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string id = "", css = "";
      if (_ID != null && _ID != "") id = " id=\"" + _ID + "\"";
      if (CssString != "") css = " style=\"" + CssString + "\"";
      builder.Append("<span" + id + css + JsEvents + ">");
      foreach (IItem item in _Children) {
        item.SerializeContent(builder, style);
      }
      builder.Append("</span>");
    }

    /// <summary>
    /// Search recursively for an element with the given ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return _Children[id];
    }

  }

}
