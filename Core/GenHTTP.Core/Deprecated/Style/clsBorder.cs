using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {
  
  /// <summary>
  /// Represents a border in CSS.
  /// </summary>
  public class Border {
    private HtmlBorder _Value, _Left, _Top, _Bottom, _Right;

    /// <summary>
    /// Create a new border object.
    /// </summary>
    internal Border() {
      _Value = new HtmlBorder(BorderPosition.All);
      _Left = new HtmlBorder(BorderPosition.Left);
      _Top = new HtmlBorder(BorderPosition.Top);
      _Bottom = new HtmlBorder(BorderPosition.Bottom);
      _Right = new HtmlBorder(BorderPosition.Right);
    }

    #region get-/setters

    /// <summary>
    /// The border of the element.
    /// </summary>
    public HtmlBorder Value {
      get { return _Value; }
    }

    /// <summary>
    /// The left border of the element.
    /// </summary>
    public HtmlBorder Left {
      get { return _Left; }
    }

    /// <summary>
    /// The top border of the element.
    /// </summary>
    public HtmlBorder Top {
      get { return _Top; }
    }

    /// <summary>
    /// The bottom border of the element.
    /// </summary>
    public HtmlBorder Bottom {
      get { return _Bottom; }
    }

    /// <summary>
    /// The right border of the element.
    /// </summary>
    public HtmlBorder Right {
      get { return _Right; }
    }

    #endregion

    /// <summary>
    /// Serialize this element to CSS code.
    /// </summary>
    /// <returns>The CSS representation of this border</returns>
    public string ToCss() {
      string css = "";
      css += Value.ToCss();
      css += Left.ToCss();
      css += Top.ToCss();
      css += Bottom.ToCss();
      css += Right.ToCss();
      return css;
    }

  }

}
