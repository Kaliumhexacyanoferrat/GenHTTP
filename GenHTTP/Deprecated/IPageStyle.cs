using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// Available type of a web page.
  /// </summary>
  public enum PageType {
    /// <summary>
    /// XHTML standard.
    /// </summary>
    XHTML,
    /// <summary>
    /// HTML standard.
    /// </summary>
    HTML
  }
  
  /// <summary>
  /// Represents the style of a web page.
  /// </summary>
  public interface IPageStyle {

    /// <summary>
    /// The type of the web page.
    /// </summary>
    PageType Type { get; set; }

    /// <summary>
    /// The encoding used to encode the web pages content.
    /// </summary>
    Encoding Encoding { get; set; }

    /// <summary>
    /// Create a copy of this object.
    /// </summary>
    /// <returns>A copy of this object</returns>
    IPageStyle Clone();

    /// <summary>
    /// The style of the header of the web page.
    /// </summary>
    IHeaderStyle Header { get; set; }

  }

}
