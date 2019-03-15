using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// A field used to mask the user's input.
  /// </summary>
  public class Passwordfield : HtmlElement, IItem {
    private string _ID;
    private string _Value;
    private string _Name;

    /// <summary>
    /// Create a new password field.
    /// </summary>
    /// <param name="id">The ID of this field</param>
    public Passwordfield(string id) {
      _ID = id;
    }

    /// <summary>
    /// Create a new password field.
    /// </summary>
    /// <param name="id">The ID of this field</param>
    /// <param name="value">The initial value of the field</param>
    public Passwordfield(string id, string value)
      : this(id) {
      _Value = value;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Textfield" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Textfield; }
    }

    /// <summary>
    /// Set or get the ID of this element.
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
    /// Set or get the name of this element.
    /// </summary>
    public string Name {
      get { return _Name; }
      set { _Name = value; }
    }

    /// <summary>
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string id = "";
      if (_ID != null) id = " id=\"" + _ID + "\"";
      string name = "";
      if (_Name != null) name = " name=\"" + _Name + "\"";
      string value = "";
      if (_Value != null) value = " value=\"" + _Value + "\"";
      string classes = "";
      if (Classes.ToCss() != null) classes = " class=\"" + Classes.ToCss() + "\"";
      string css = (CssString != "") ? " style=\"" + CssString + "\"" : "";
      builder.Append("<input type=\"password\"" + id + JsEvents + name + css + value + classes + " />");
    }

    /// <summary>
    /// Retrieve an element by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
