using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {
  
  /// <summary>
  /// Plain text in HTML.
  /// </summary>
  public class Text : IItem {
    private string _Value;

    /// <summary>
    /// Create a new text element.
    /// </summary>
    /// <param name="value">The text value</param>
    /// <remarks>This class will also escape some special characters, which should not be used in (X)HTML (like umlauts).</remarks>
    public Text(string value) {
      Value = value;
    }

    /// <summary>
    /// Get or set the value of this text element.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { 
        _Value = value;
        if (_Value != null) {
          // Escape some special characters
          _Value = _Value.Replace("Ä", "&Auml;");
          _Value = _Value.Replace("Ö", "&Ouml;");
          _Value = _Value.Replace("Ü", "&Uuml;");
          _Value = _Value.Replace("ä", "&auml;");
          _Value = _Value.Replace("ö", "&ouml;");
          _Value = _Value.Replace("ü", "&uuml;");
          _Value = _Value.Replace("ß", "&szlig;");
        }
      }
    }

    /// <summary>
    /// Escapes some HTML special characters (e.g. quotes).
    /// </summary>
    /// <returns>The modified text item</returns>
    /// <remarks>This method returns the current <see cref="IItem" /> to allow a short syntax. Example:
    /// <example>
    /// <code>
    /// Div content = new Div();
    /// content.Children.Add(new Text("Some \"Text\"").EscapeHtml());
    /// </code>
    /// </example>
    /// </remarks>
    public IItem EscapeHtml() {
      _Value = _Value.Replace("&", "&amp;");
      _Value = _Value.Replace("<", "&lt;");
      _Value = _Value.Replace(">", "&gt;");
      _Value = _Value.Replace("\"", "&quot;");
      _Value = _Value.Replace("'", "&apos;");
      return this;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Text" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Text; }
    }

    /// <summary>
    /// Won't work for this element.
    /// </summary>
    public string ID {
      get { return null; }
      set { }
    }

    /// <summary>
    /// Serialize the content of this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      if (_Value != null) builder.Append(_Value);
    }

    /// <summary>
    /// Won't work for this element.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>null</returns>
    public IItem GetElementByID(string id) { return null; }

  }

}
