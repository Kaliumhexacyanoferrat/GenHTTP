using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// A HTML image.
  /// </summary>
  public class Img : HtmlElement, IItem {
    private string _ID;
    private string _Alt;
    private string _File;
    private VAlign _VAlign = VAlign.None;

    /// <summary>
    /// Create a new image.
    /// </summary>
    /// <param name="file">The URL of the image</param>
    /// <param name="alt">The description of this image (required by the HTML standard)</param>
    public Img(string file, string alt) {
      _Alt = alt;
      _File = file;
    }

    /// <summary>
    /// Create a new image.
    /// </summary>
    /// <param name="id">The ID of the element</param>
    /// <param name="file">The URL of the image</param>
    /// <param name="alt">The description of this image (required by the HTML standard)</param>
    public Img(string id, string file, string alt) : this(file, alt) {
      _ID = id;
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Image" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Image; }
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
    /// Set or get the vertical alignment of this image.
    /// </summary>
    public VAlign VAlign {
      get { return _VAlign; }
      set { _VAlign = value; }
    }

    /// <summary>
    /// Serialize the content of this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to write to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string id = "";
      if (_ID != null) id = " id=\"" + _ID + "\"";
      string css = "";
      if (CssString != "") css = " style=\"" + CssString + "\"";
      string valign = "";
      switch (_VAlign) {
        case VAlign.Bottom: valign = " align=\"bottom\""; break;
        case VAlign.Middle: valign = " align=\"middle\""; break;
        case VAlign.Top: valign = " align=\"top\""; break;
      }
      builder.Append("<img src=\"" + _File + "\" alt=\"" + _Alt + "\"" + id + JsEvents + css + valign + " border=\"0\" />");
    }

    /// <summary>
    /// Retrieve an item by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element</returns>
    public IItem GetElementByID(string id) {
      if (id == _ID) return this;
      return null;
    }

  }

}
