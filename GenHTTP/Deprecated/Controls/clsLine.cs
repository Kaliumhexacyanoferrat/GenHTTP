using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// Represents a simple line
  /// </summary>
  public class Line : HtmlElement, IItem {
    private string _ID;

    /// <summary>
    /// The type of the element. Should be <see cref="ItemType.Line" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Line; }
    }

    /// <summary>
    /// The ID of this element.
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
    /// <param name="builder">The string builder to write to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      builder.Append("<hr " + Classes.ToCss() + " />");
    }

    /// <summary>
    /// Search for a element with the given ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it couldn't be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
