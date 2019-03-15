using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// A submit button on a page, used to send the content of a form to the server.
  /// </summary>
  public class SubmitButton : HtmlElement, IItem {
    private string _ID;
    private string _Value;
    private string _Name;

    /// <summary>
    /// Create a new submit button.
    /// </summary>
    /// <param name="value">The caption of this button</param>
    public SubmitButton(string value) {
      _Value = value;
    }

    /// <summary>
    /// Create a new submit button.
    /// </summary>
    /// <param name="id">The ID of this button</param>
    /// <param name="value">The caption of the button</param>
    public SubmitButton(string id, string value) : this(value) {
      _ID = id;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Button" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Button; }
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
      get {
        return _Name;
      }
      set {
        _Name = value;
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
      if (_ID != null) id = " id=\"" + _ID + "\"";
      string name = "";
      if (_Name != null) name = " name=\"" + _Name + "\"";
      string cls = "";
      if (Classes.ToCss() != "") cls = " class=\"" + Classes.ToCss() + "\"";
      builder.Append("<input type=\"submit\"" + id + name + cls + css + " value=\"" + _Value + "\" />");
    }

    /// <summary>
    /// Retrieve a HTML element by its ID.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
