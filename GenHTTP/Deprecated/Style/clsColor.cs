using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {
  
  /// <summary>
  /// Represents a HTML color.
  /// </summary>
  public class Color {
    private HtmlColor _BackgroundColor;
    private HtmlColor _Color;

    internal Color() {}

    #region get-/setters

    /// <summary>
    /// The background color.
    /// </summary>
    public HtmlColor Background {
      get { return _BackgroundColor; }
      set { _BackgroundColor = value; }
    }

    /// <summary>
    /// The foreground color.
    /// </summary>
    public HtmlColor Foreground {
      get { return _Color; }
      set { _Color = value; }
    }

    #endregion

    /// <summary>
    /// Retrieve the CSS representation of this color.
    /// </summary>
    /// <returns>The CSS representation of this color</returns>
    internal string ToCss() {
      string toreturn = "";
      if (_BackgroundColor != null) toreturn += "background-color: " + _BackgroundColor.ColorString + "; ";
      if (_Color != null) toreturn += "color: " + _Color.ColorString + "; ";
      return toreturn;
    }

  }

}
