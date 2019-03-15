using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {

  /// <summary>
  /// A default page style which can be used by projects.
  /// </summary>
  /// <remarks>
  /// Uses XHTML and UTF8.
  /// </remarks>
  public class DefaultPageStyle : IPageStyle {
    private DefaultHeaderStyle _Header;

    /// <summary>
    /// Create a new default style.
    /// </summary>
    public DefaultPageStyle() {
      _Header = new DefaultHeaderStyle();
    }

    /// <summary>
    /// The type of this page. Should be XHTML.
    /// </summary>
    public PageType Type {
      get {
        return PageType.XHTML;
      }
      set {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// The encoding of this page. Should be UTF-8.
    /// </summary>
    public Encoding Encoding {
      get {
        return Encoding.UTF8;
      }
      set {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Clone this object.
    /// </summary>
    /// <returns>The cloned object</returns>
    public IPageStyle Clone() {
      return this;
    }

    /// <summary>
    /// The header style of the default page style.
    /// </summary>
    public IHeaderStyle Header {
      get {
        return _Header;
      }
      set {
        throw new NotImplementedException();
      }
    }

  }

  /// <summary>
  /// The default header style, using UTF-8.
  /// </summary>
  public class DefaultHeaderStyle : IHeaderStyle {

    #region IHeaderStyle Member

    /// <summary>
    /// The HTML encoding. Should be UTF-8.
    /// </summary>
    public HtmlEncoding HtmlEncoding {
      get {
        return HtmlEncoding.utf_8;
      }
      set {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Clone this header style object.
    /// </summary>
    /// <returns>A copy of this object</returns>
    public IHeaderStyle Clone() {
      return new DefaultHeaderStyle();
    }

    #endregion
  }

}
