using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// Represents a HTML or XHTML page, which can be sent to the connected
  /// client.
  /// </summary>
  public class Page {
    private Header _Header;
    private Body _Body;
    private IPageStyle _Style;

    /// <summary>
    /// Create a new (X)HTML page.
    /// </summary>
    /// <param name="style">The style to use for this page (encodings, ...)</param>
    public Page(IPageStyle style) {
      _Style = style; 
      _Header = new Header(this);
      _Body = new Body(this);
    }

    #region get-/setters

    /// <summary>
    /// Retrieve the content of this page.
    /// </summary>
    /// <remarks>
    /// This object will generate a complete (X)HTML page with the given
    /// preferences and content.
    /// </remarks>
    public byte[] SerializedContent {
      get {
        string nl = Environment.NewLine;
        StringBuilder toprint = new StringBuilder();
        if (_Style.Type == PageType.HTML) {
          toprint.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">" + nl + nl);
          toprint.Append("<html>" + nl + nl );
        }
        else {
          string encoding = Enum.GetName(typeof(HtmlEncoding), Style.Header.HtmlEncoding).Replace("_", "-");
          if (encoding == "shift-jis") encoding = "shift_jis";
          toprint.Append("<?xml version=\"1.0\" encoding=\"" + encoding + "\" ?>" + nl);
          toprint.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + nl + nl);
          toprint.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">" + nl + nl);
        }
        // print HTML header
        toprint.Append("<head>" + nl);
        _Header.SerializeContent(toprint, _Style);
        toprint.Append(nl);
        toprint.Append("</head>" + nl + nl);
        // print body
        _Body.SerializeContent(toprint, Style);
        toprint.Append(nl + nl);
        // finish page
        toprint.Append("</html>");
        return _Style.Encoding.GetBytes(toprint.ToString());
      }
    }

    /// <summary>
    /// The style to use for this page.
    /// </summary>
    public IPageStyle Style {
      get {
        return _Style;
      }
    }

    /// <summary>
    /// The HTML header of this page.
    /// </summary>
    public Header Header {
      get {
        return _Header;
      }
    }

    /// <summary>
    /// The document's body.
    /// </summary>
    public Body Body {
      get {
        return _Body;
      }
    }

    #endregion

  }

}
