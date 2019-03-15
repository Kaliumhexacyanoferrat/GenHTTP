using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
 
  /// <summary>
  /// Represents a large text field in HTML.
  /// </summary>
  public class Textarea : HtmlElement, IItem {
    private string _ID;
    private string _Value;

    /// <summary>
    /// Create a new text field.
    /// </summary>
    /// <param name="value">The initial content of the text field</param>
    public Textarea(string value) {
      _Value = value;
    }

    /// <summary>
    /// Create a new text field.
    /// </summary>
    /// <param name="id">The ID of the field</param>
    /// <param name="value">The initial value of the text field</param>
    public Textarea(string id, string value) : this(value) {
      _ID = id;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Textarea" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Textarea; }
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
    /// Get or set the value of the text field.
    /// </summary>
    public string Value {
      get {
        return _Value;
      }
      set {
        _Value = value;
      }
    }

    /// <summary>
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string css = "";
      if (CssString != "") css = " style=\"" + CssString + "\"";
      string id = "";
      if (_ID != "") id = " id=\"" + _ID + "\" name=\"" + _ID + "\"";
      string cls = "";
      if (Classes.ToCss() != "") cls = " class=\"" + Classes.ToCss() + "\"";
      builder.Append("<textarea" + css + id + cls + ">" + _Value + "</textarea>");
    }

    /// <summary>
    /// Retrieve a HTML element by its ID.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>The requested item or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
