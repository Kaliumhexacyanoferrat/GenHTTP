using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenHTTP.Style;
using GenHTTP;

namespace GenHTTP.Controls {

  /// <summary>
  /// The format of a plain text element.
  /// </summary>
  public enum FontWeight {
    /// <summary>
    /// Normal weight.
    /// </summary>
    Normal,
    /// <summary>
    /// No specification.
    /// </summary>
    None,
    /// <summary>
    /// Bold font.
    /// </summary>
    Bold
  }

  /// <summary>
  /// A HTML element, containing some basic definitions for items.
  /// </summary>
  /// /// <remarks>
  /// Not all HTML elements will support every feature of this class. So choose wisely ..
  /// </remarks>
  public class HtmlElement {
    private Position _Padding, _Margin;
    private Color _Color;
    private CssClassCollection _Classes;
    private Size _Size;
    private Border _Border;
    private Font _Font;
    private Align _Align;
    private FontWeight _Weight;
    
    private string _OnClick, _OnChange;

    /// <summary>
    /// Create a new HTML element.
    /// </summary>
    public HtmlElement() {
      _Padding = new Position("padding");
      _Margin = new Position("margin");
      _Color = new Color();
      _Classes = new CssClassCollection();
      _Size = new Size();
      _Border = new Border();
      _Font = new Font();
      _Align = Align.None;
      _Weight = FontWeight.None;
    }

    /// <summary>
    /// The padding of this element.
    /// </summary>
    public Position Padding {
      get {
        return _Padding;
      }
    }

    /// <summary>
    /// The margin of this element.
    /// </summary>
    public Position Margin {
      get {
        return _Margin;
      }
    }

    /// <summary>
    /// The alignment of this element.
    /// </summary>
    public Align Align {
      get { return _Align; }
      set { _Align = value; }
    }

    /// <summary>
    /// The color of this element.
    /// </summary>
    public Color Color {
      get {
        return _Color;
      }
    }

    /// <summary>
    /// The border of this element.
    /// </summary>
    public Border Border {
      get {
        return _Border;
      }
    }

    /// <summary>
    /// The CSS classes used by this element.
    /// </summary>
    public CssClassCollection Classes {
      get {
        return _Classes;
      }
    }

    /// <summary>
    /// The size of this element.
    /// </summary>
    public Size Size {
      get {
        return _Size;
      }
    }

    /// <summary>
    /// The font used for this element.
    /// </summary>
    public Font Font {
      get {
        return _Font;
      }
    }

    /// <summary>
    /// Code which will be executed on click on this element.
    /// </summary>
    public string OnClick {
      get { return _OnClick; }
      set { _OnClick = value; }
    }

    /// <summary>
    /// Code will be executed when the content of the element has changed.
    /// </summary>
    public string OnChange {
      get { return _OnChange; }
      set { _OnChange = value; }
    }

    /// <summary>
    /// The font style of this element.
    /// </summary>
    public FontWeight FontWeight {
      get { return _Weight; }
      set { _Weight = value; }
    }

    internal string JsEvents {
      get {
        string retval = "";
        if (OnClick != null) retval += " onclick=\"" + OnClick + "\"";
        if (OnChange != null) retval += " onchange=\"" + OnChange + "\"";
        return retval;
      }
    }

    internal string CssString {
      get {
        string align = "";
        switch (_Align) {
          case Align.Center: align = "text-align: center; "; break;
          case Align.Left: align = "text-align: left; "; break;
          case Align.Right: align = "text-align: right; "; break;
        }
        string weight = "";
        switch (_Weight) {
          case FontWeight.Bold: weight = "font-weight: bold; "; break;
          case FontWeight.Normal: weight = "font-weight: normal; "; break;
        }
        string css = _Padding.ToCss() + _Margin.ToCss() + _Color.ToCss() + _Size.ToCss() + _Border.ToCss() + _Font.ToCss() + align + weight;
        if (css.Length > 0) {
          if (css.EndsWith(" ")) return css.Substring(0, css.Length - 1);
          return css;
        }
        return "";
      }
    }

  }

}
