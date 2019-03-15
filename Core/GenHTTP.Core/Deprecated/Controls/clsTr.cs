using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// Represents a line in a HTML table.
  /// </summary>
  public class Tr : HtmlElement, IItem {
    private ItemCollection _Children;
    private string _ID;

    /// <summary>
    /// Create a new line.
    /// </summary>
    public Tr() {
      _Children = new ItemCollection();
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Tr" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Tr; }
    }

    /// <summary>
    /// Retrieve all sub childs of this element.
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
    /// Serialize the content of this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      builder.Append("  <tr>" + Environment.NewLine);
      foreach (IItem child in _Children) {
        child.SerializeContent(builder, style);
      }
      builder.Append("  </tr>" + Environment.NewLine);
    }

    /// <summary>
    /// Search recursively for an element, specified by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return _Children[id];
    }

  }

}
