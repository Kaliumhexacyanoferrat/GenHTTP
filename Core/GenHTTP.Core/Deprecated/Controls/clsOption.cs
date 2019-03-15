using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// Represents a option control in HTML.
  /// </summary>
  public class Option : HtmlElement, IItem {
    private string _Value;
    private string _ID;
    private bool _Selected = false;

    /// <summary>
    /// Create a new option element.
    /// </summary>
    /// <param name="value">The value of this option</param>
    public Option(string value) {
      if (value == null) _Value = "";
      else _Value = value;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Option" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Option;  }
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
    /// Specify, whether this option is selected or not.
    /// </summary>
    public bool Selected {
      get { return _Selected; }
      set { _Selected = value; }
    }

    /// <summary>
    /// Get or set the value of this list entry.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = (value == null) ? "" : value; }
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
      string selected = (_Selected) ? " selected=\"selected\"" : "";
      builder.Append("  <option" + selected + css + id + JsEvents + ">" + _Value + "</option>");
    }

    /// <summary>
    /// Search for the element with the given ID.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (id == _ID) return this;
      return null;
    }

  }

}
