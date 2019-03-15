using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// Represents a hidden field in HTML.
  /// </summary>
  public class Hidden : IItem {
    private string _ID;
    private string _Name;
    private string _Value;

    /// <summary>
    /// Create a new hidden field.
    /// </summary>
    /// <param name="name">The name of the field to create</param>
    /// <param name="value">The value of the new field</param>
    public Hidden(string name, string value) {
      _Name = name;
      _Value = value;
    }

    /// <summary>
    /// Set or get the content of this field.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// Get or set the name of this field.
    /// </summary>
    public string Name {
      get { return _Name; }
      set { _Name = value; }
    }

    /// <summary>
    /// The type of this item. Should be <see cref="ItemType.Hidden" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Hidden; }
    }

    /// <summary>
    /// Get or set the ID of this item.
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
    /// Serialize this item to a string builder.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string id = (_ID != null) ? " id=\"" + _ID + "\"" : "";
      builder.Append("<input type=\"hidden\" name=\"" + _Name + "\"" + id + " value=\"" + _Value + "\" />");
    }

    /// <summary>
    /// Search for an element by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
