using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// A text field on a HTML web page.
  /// </summary>
  public class Textfield: HtmlElement, IItem {
    private string _ID;
    private string _Value;
    private string _Name;

    /// <summary>
    /// Create a new text field.
    /// </summary>
    /// <param name="id">The ID of the text field</param>
    public Textfield(string id) {
      _ID = id;
    }

    /// <summary>
    /// Create a new text field.
    /// </summary>
    /// <param name="id">The ID of the text field</param>
    /// <param name="value">The initial value of the field</param>
    public Textfield(string id, string value) : this(id) {
      _Value = value;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Textfield" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Textfield; }
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
    /// Get or set the name of this element.
    /// </summary>
    public string Name {
      get { return _Name; }
      set { _Name = value; }
    }

    /// <summary>
    /// The initial value of the text field.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
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
      builder.Append("<input type=\"text\"" + id + name + css + JsEvents + value + classes + " />");
    }

    /// <summary>
    /// Retrieve an item by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
