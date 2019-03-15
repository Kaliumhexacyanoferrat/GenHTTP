using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// Represents a (X)HTML headline.
  /// </summary>
  public class Headline : IItem {
    private string _ID;
    private ushort _Size = 1;
    private string _Text;

    /// <summary>
    /// Create a new headline.
    /// </summary>
    /// <param name="text">The text to print</param>
    public Headline(string text) {
      _Text = text;
    }

    /// <summary>
    /// Create a new headline, using a other size than 1.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <param name="size">The size of the headline (1-6)</param>
    public Headline(string text, ushort size) : this(text) {
      _Size = size;
    }

    /// <summary>
    /// Create a mew headline with a given id.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <param name="size">The size of the headline (1-6)</param>
    /// <param name="id">The id of the headline element</param>
    public Headline(string text, ushort size, string id) : this(text, size) {
      _ID = id;
    }

    /// <summary>
    /// The type of the element.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Headline; }
    }

    /// <summary>
    /// The ID of the element.
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
    /// Serialize this element to a StringBuilder.
    /// </summary>
    /// <param name="builder">The StringBuilder to append the content to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      builder.Append("<h" + _Size + ((_ID != null) ? " id=\"" + _ID + "\"" : "") + ">" + _Text + "</h" + _Size + ">");
    }

    /// <summary>
    /// Check whether this headline uses the given ID.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>A reference to this element, if this.ID == id, otherwise null</returns>
    public IItem GetElementByID(string id) {
      if (id == _ID) return this;
      return null;
    }

  }

}
