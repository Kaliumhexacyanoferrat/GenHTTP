using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {

  /// <summary>
  /// CSS property border-type,
  /// </summary>
  public enum BorderType {
    /// <summary>
    /// A solid line.
    /// </summary>
    Solid,
    /// <summary>
    /// A dashed line.
    /// </summary>
    Dashed
  }

  /// <summary>
  /// Values used to position borders in CSS.
  /// </summary>
  public enum BorderPosition {
    /// <summary>
    /// Everywhere around the element.
    /// </summary>
    All,
    /// <summary>
    /// On the left side.
    /// </summary>
    Left,
    /// <summary>
    /// On the right side.
    /// </summary>
    Right,
    /// <summary>
    /// On top.
    /// </summary>
    Bottom,
    /// <summary>
    /// On bottom.
    /// </summary>
    Top
  }

  /// <summary>
  /// Represents a border in HTML.
  /// </summary>
  public class HtmlBorder {
    private int? _Width;
    private BorderType _Type = BorderType.Solid;
    private BorderPosition _Position = BorderPosition.All;
    private HtmlColor _Color = HtmlColor.Black;

    internal HtmlBorder(BorderPosition position) {
      _Position = position;
    }

    #region get-/setters

    /// <summary>
    /// The border's width.
    /// </summary>
    public int? Width {
      get { return _Width; }
      set { _Width = value; }
    }

    /// <summary>
    /// The border's type.
    /// </summary>
    public BorderType Type {
      get { return _Type; }
      set { _Type = value; }
    }

    /// <summary>
    /// The border's position.
    /// </summary>
    public BorderPosition Position {
      get { return _Position; }
      set { _Position = value; }
    }

    /// <summary>
    /// The border's color.
    /// </summary>
    public HtmlColor Color {
      get { return _Color; }
      set { _Color = value; }
    }

    #endregion

    internal string ToCss() {
      if (!_Width.HasValue) return "";
      string position = "border";
      switch (_Position) {
        case BorderPosition.Bottom: position += "-bottom"; break;
        case BorderPosition.Left: position += "-left"; break;
        case BorderPosition.Top: position += "-top"; break;
        case BorderPosition.Right: position += "-right"; break;
      }
      position += ": " + _Width + "px ";
      switch (_Type) {
        case BorderType.Dashed: position += "dashed "; break;
        case BorderType.Solid: position += "solid "; break;
      }
      return position + Color.ColorString + "; ";
    }

  }

}
