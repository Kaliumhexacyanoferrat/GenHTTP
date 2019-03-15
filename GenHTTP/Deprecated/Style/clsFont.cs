using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {
  
  /// <summary>
  /// Represents a CSS font.
  /// </summary>
  public class Font {
    private int? _Size;
    private string _FontFamily;

    /// <summary>
    /// Create a new font object.
    /// </summary>
    internal Font() {

    }

    #region get-/setters

    /// <summary>
    /// Get or set the font size.
    /// </summary>
    public int? Size {
      get { return _Size; }
      set { _Size = value; }
    }

    /// <summary>
    /// Get or set the font family.
    /// </summary>
    public string FontFamily {
      get { return _FontFamily; }
      set { _FontFamily = value; }
    }

    #endregion

    /// <summary>
    /// Serialize the font to a CSS string.
    /// </summary>
    /// <returns>The serialized representation of this font</returns>
    internal string ToCss() {
      string css = "";
      if (_Size.HasValue) css += "font-size: " + _Size.Value + "pt; ";
      if (_FontFamily != null) css += "font-family: " + _FontFamily + "; ";
      return css;
    }

  }

}
