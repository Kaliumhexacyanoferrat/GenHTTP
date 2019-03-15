using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// A list containing options.
  /// </summary>
  public class Select : HtmlElement, IItem {
    private string _ID;
    private ushort _ListSize = 1;
    private List<Option> _Options;

    /// <summary>
    /// Create a new select section.
    /// </summary>
    public Select() {
      _Options = new List<Option>();
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Select" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Select; }
    }

    /// <summary>
    /// Retrieve the options assigned to this select section.
    /// </summary>
    public List<Option> Options {
      get { return _Options; }
    }

    /// <summary>
    /// Get or set the size of the box to display.
    /// </summary>
    public ushort ListSize {
      get { return _ListSize; }
      set { _ListSize = value; }
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
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string nl = Environment.NewLine;
      string css = "";
      if (CssString != "") css = " style=\"" + CssString + "\"";
      string id = "";
      if (_ID != null) id = " id=\"" + _ID + "\" name=\"" + _ID + "\"";
      string cls = "";
      if (Classes.ToCss() != "") cls = " class=\"" + Classes.ToCss() + "\"";
      builder.Append(nl + nl + "<select size=\"" + _ListSize + "\"" + css + cls + id + JsEvents + ">" + nl);
      foreach (Option option in _Options) {
        option.SerializeContent(builder, style);
        builder.Append(nl);
      }
      builder.Append("</select>" + nl + nl);
    }

    /// <summary>
    /// Search recursively for an element.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      foreach (Option o in _Options) {
        IItem tmp = o.GetElementByID(id);
        if (tmp != null) return tmp;
      }
      return null;
    }

  }

}
