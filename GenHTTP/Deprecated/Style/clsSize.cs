using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {

  /// <summary>
  /// Represents sizes in HTML.
  /// </summary>
  public class Size {
    private string _Left = "", _Top = "", _Height = "", _Width = "";

    #region get-/setters

    /// <summary>
    /// The left position of this element.
    /// </summary>
    public string Left {
      get { return _Left; }
      set { _Left = value; }
    }

    /// <summary>
    /// The top position of this element.
    /// </summary>
    public string Top {
      get { return _Top; }
      set { _Top = value; }
    }

    /// <summary>
    /// The height of this element.
    /// </summary>
    public string Height {
      get { return _Height; }
      set { _Height = value; }
    }

    /// <summary>
    /// The width of this element.
    /// </summary>
    public string Width {
      get { return _Width; }
      set { _Width = value; }
    }

    #endregion

    /// <summary>
    /// Serialize the size information to a CSS string.
    /// </summary>
    /// <returns>The CSS representation of the size information</returns>
    public string ToCss() {
      string css = "";
      if (_Left != null && _Left.Length > 0) css += "left: " + _Left + "; ";
      if (_Top != null && _Top.Length > 0) css += "top: " + _Top + "; ";
      if (_Width != null && _Width.Length > 0) css += "width: " + _Width + "; ";
      if (_Height != null && _Height.Length > 0) css += "height: " + _Height + "; ";
      return css;
    }

  }

}
