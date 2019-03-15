/*

Updated: 2009/10/19

2009/10/19  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Style;
using GenHTTP.Utilities;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Defines an element with a specific style.
  /// </summary>
  public abstract class StyledElement : Element {
    private ElementFont _Font;
    private ElementSize _Width;
    private ElementSize _Height;
    private ElementSize _LineHeight;
    private ElementSize _LetterSpacing;
    private ElementSize _WordSpacing;
    private ElementSize _TextIndent;
    private ElementPosition _Padding;
    private ElementPosition _Margin;
    private ElementColor _Color;
    private ElementColor _BackgroundColor;
    private ElementBorderCollection _Border;
    private ElementTextAlign _TextAlign = ElementTextAlign.Unspecified;
    private ElementVerticalAlign _VerticalAlign = ElementVerticalAlign.Unspecified;
    private ElementFontWeight _FontWeight = ElementFontWeight.Unspecified;
    private ElementFontStyle _FontStyle = ElementFontStyle.Unspecified;
    private ElementTextDecoration _TextDecoration = ElementTextDecoration.Unspecified;
    private ElementClear _Clear = ElementClear.Unspecified;
    private ElementFlow _Flow = ElementFlow.Unspecified; 
    private string _BackgroundImage;
    private string _Additional;
    private List<string> _Classes;

    #region Constructors

    /// <summary>
    /// Initializes the class collection.
    /// </summary>
    public StyledElement() {
      _Classes = new List<string>();
    }

    #endregion

    #region Class handling

    /// <summary>
    /// Add a CSS class to this element.
    /// </summary>
    /// <param name="className">The name of the class</param>
    public void AddClass(string className) {
      if (className == null) throw new ArgumentNullException();
      if (!_Classes.Contains(className)) _Classes.Add(className);
    }

    /// <summary>
    /// Remove a CSS class from this element.
    /// </summary>
    /// <param name="className">The name of the class to remove</param>
    public void RemoveClass(string className) {
      if (className == null) throw new ArgumentNullException();
      if (_Classes.Contains(className)) _Classes.Remove(className);
    }

    /// <summary>
    /// The number of classes in this styled element.
    /// </summary>
    public int ClassCount {
      get { return _Classes.Count; }
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The font of the element.
    /// </summary>
    public ElementFont Font {
      get { return _Font; }
      set { _Font = value; }
    }

    /// <summary>
    /// The width of the element.
    /// </summary>
    public ElementSize Width {
      get { return _Width; }
      set { _Width = value; }
    }

    /// <summary>
    /// The height of the element.
    /// </summary>
    public ElementSize Height {
      get { return _Height; }
      set { _Height = value; }
    }

    /// <summary>
    /// The padding of the element.
    /// </summary>
    public ElementPosition Padding {
      get { return _Padding; }
      set { _Padding = value; }
    }

    /// <summary>
    /// The margin of the element.
    /// </summary>
    public ElementPosition Margin {
      get { return _Margin; }
      set { _Margin = value; }
    }

    /// <summary>
    /// You can add additional CSS statements here, which
    /// are not supported by the object framework yet.
    /// </summary>
    public string Additional {
      get { return _Additional; }
      set { _Additional = value; }
    }

    /// <summary>
    /// The alignment of the text within this element.
    /// </summary>
    public ElementTextAlign TextAlign {
      get { return _TextAlign; }
      set { _TextAlign = value; }
    }

    /// <summary>
    /// The color of the element.
    /// </summary>
    public ElementColor Color {
      get { return _Color; }
      set { _Color = value; }
    }

    /// <summary>
    /// The background-color of the element.
    /// </summary>
    public ElementColor BackgroundColor {
      get { return _BackgroundColor; }
      set { _BackgroundColor = value; }
    }

    /// <summary>
    /// The border of the element.
    /// </summary>
    public ElementBorderCollection Border {
      get { return _Border; }
      set { _Border = value; }
    }

    /// <summary>
    /// The URL of the background image of the element.
    /// </summary>
    public string BackgroundImage {
      get { return _BackgroundImage; }
      set { _BackgroundImage = value; }
    }

    /// <summary>
    /// The vertical text alignment.
    /// </summary>
    public ElementVerticalAlign VerticalTextAlign {
      get { return _VerticalAlign; }
      set { _VerticalAlign = value; }
    }

    /// <summary>
    /// The font weight of this element.
    /// </summary>
    public ElementFontWeight FontWeight {
      get { return _FontWeight; }
      set { _FontWeight = value; }
    }

    /// <summary>
    /// The font style of the element.
    /// </summary>
    public ElementFontStyle FontStyle {
      get { return _FontStyle; }
      set { _FontStyle = value; }
    }

    /// <summary>
    /// The distance between lines.
    /// </summary>
    public ElementSize LineHeight {
      get { return _LineHeight; }
      set { _LineHeight = value; }
    }

    /// <summary>
    /// The distance between single letters.
    /// </summary>
    public ElementSize LetterSpacing {
      get { return _LetterSpacing; }
      set { _LetterSpacing = value; }
    }

    /// <summary>
    /// The distance between single words.
    /// </summary>
    public ElementSize WordSpacing {
      get { return _WordSpacing; }
      set { _WordSpacing = value; }
    }

    /// <summary>
    /// The text decoration of the element.
    /// </summary>
    public ElementTextDecoration TextDecoration {
      get { return _TextDecoration; }
      set { _TextDecoration = value; }
    }

    /// <summary>
    /// The text indent of the element.
    /// </summary>
    public ElementSize TextIndent {
      get { return _TextIndent; }
      set { _TextIndent = value; }
    }

    /// <summary>
    /// Clear flowing?
    /// </summary>
    public ElementClear Clear {
      get { return _Clear; }
      set { _Clear = value; }
    }

    /// <summary>
    /// The flow of the element.
    /// </summary>
    public ElementFlow Float {
      get { return _Flow; }
      set { _Flow = value; }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Convert the style of this element into a CSS string.
    /// </summary>
    /// <returns>The CSS representation of the style</returns>
    internal string ToCss() {
      return ToCss(true);
    }

    /// <summary>
    /// Convert the style of this element into a CSS string.
    /// </summary>
    /// <param name="embrance">Specify, whether the code should get embranced with a 'style' attribute</param>
    /// <returns>The CSS representation of the style</returns>
    internal string ToCss(bool embrance) {
      string ret = "";
      // Font
      if (_Font != null) ret += _Font.ToCss();
      // Width, height, padding, etc.
      if (_Width != null) ret += SizeHelper("width", _Width);
      if (_Height != null) ret += SizeHelper("height", _Height);
      if (_LineHeight != null) ret += SizeHelper("line-height", _LineHeight);
      if (_LetterSpacing != null) ret += SizeHelper("letter-spacing", _LetterSpacing);
      if (_WordSpacing != null) ret += SizeHelper("word-spacing", _WordSpacing);
      if (_TextIndent != null) ret += SizeHelper("text-indent", _TextIndent);
      if (_Padding != null) ret += _Padding.ToCss("padding");
      if (_Margin != null) ret += _Margin.ToCss("margin");
      // colors
      if (_Color != null) ret += " color: " + _Color.ToCss() + ";";
      if (_BackgroundColor != null) ret += " background-color: " + _BackgroundColor.ToCss() + ";";
      // text-alignment
      if (_TextAlign != ElementTextAlign.Unspecified) ret += " text-align: " + _TextAlign.ToString().ToLower() + ";";
      if (_VerticalAlign != ElementVerticalAlign.Unspecified) ret += " vertical-align: " + GetVerticalAlignment(_VerticalAlign) + ";";
      // font-weight
      if (_FontWeight != ElementFontWeight.Unspecified) ret += " font-weight: " + _FontWeight.ToString().ToLower() + ";";
      // font style
      if (_FontStyle != ElementFontStyle.Unspecified) ret += " font-style: " + _FontStyle.ToString().ToLower() + ";";
      if (_TextDecoration != ElementTextDecoration.Unspecified) ret += " text-decoration: " + GetTextDecoration(_TextDecoration) + ";";
      // flowing
      if (_Clear != ElementClear.Unspecified) ret += " clear: " + _Clear.ToString().ToLower() + ";";
      if (_Flow != ElementFlow.Unspecified) ret += " float: " + _Flow.ToString().ToLower() + ";";
      // border
      if (_Border != null) ret += _Border.ToCss();
      // background-image
      if (_BackgroundImage != null && _BackgroundImage.Length > 0) ret += " background-image:url('" + _BackgroundImage + "');";
      // additional stuff
      if (_Additional != null && _Additional.Length > 0) ret += " " + _Additional + ";";
      // return the result
      if (ret.Length > 0) ret = ret.Substring(1); // remove the leading space
      if (embrance && ret.Length > 0) return " style=\"" + ret + "\"";
      return ret;
    }

    /// <summary>
    /// Retrieve a class string for this element.
    /// </summary>
    /// <returns>The requested class string</returns>
    public string ToClassString() {
      string ret = "";
      foreach (string c in _Classes) ret += " " + c;
      if (ret.Length == 0) return ret;
      ret = " class=\"" + ret.Substring(1) + "\"";
      return ret;
    }

    private string SizeHelper(string attrib, ElementSize size) {
      if (size == null) return "";
      string css = size.ToCss();
      if (css.Length > 0) return " " + attrib + ": " + css + ";";
      return "";
    }

    /// <summary>
    /// Convert a vertical alignment type into a string.
    /// </summary>
    /// <param name="align">The alignment to convert</param>
    /// <returns>The CSS representation of the alignment</returns>
    public static string GetVerticalAlignment(ElementVerticalAlign align) {
      string ret = align.ToString().ToLower();
      if (ret.StartsWith("text")) return "text-" + ret.Substring(4);
      return ret;
    }

    /// <summary>
    /// Convert a text decoration type into a string.
    /// </summary>
    /// <param name="decoration">The text decoration to convert</param>
    /// <returns>The CSS representation of the decoration</returns>
    public static string GetTextDecoration(ElementTextDecoration decoration) {
      string ret = decoration.ToString().ToLower();
      if (decoration == ElementTextDecoration.LineThrough) return "line-trough";
      return ret;
    }

    #endregion

  }

}
