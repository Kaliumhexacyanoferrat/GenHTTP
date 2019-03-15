using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace GenHTTP.Style {
  
  /// <summary>
  /// Represents a HTML color.
  /// </summary>
  public class HtmlColor {
    private byte _R, _G, _B;

    /// <summary>
    /// Create a new HTML color object.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    public HtmlColor(byte r, byte g, byte b) {
      _R = r;
      _G = g;
      _B = b;
    }

    #region get-/setters

    /// <summary>
    /// Red
    /// </summary>
    public byte R {
      get { return _R; }
      set { _R = value; }
    }

    /// <summary>
    /// Green
    /// </summary>
    public byte G {
      get { return _G; }
      set { _G = value; }
    }

    /// <summary>
    /// Blue
    /// </summary>
    public byte B {
      get { return _B; }
      set { _B = value; }
    }

    /// <summary>
    /// Retrieve the color string (with a leading "#").
    /// </summary>
    public string ColorString {
      get {
        string red = _R.ToString("X");
        string green = _G.ToString("X");
        string blue = _B.ToString("X");
        if (red.Length == 1) red = "0" + red;
        if (green.Length == 1) green = "0" + green;
        if (blue.Length == 1) blue = "0" + blue;
        return "#" + red + green + blue; 
      }
    }

    /// <summary>
    /// Retrieve a darkened color (30% of the original color).
    /// </summary>
    public HtmlColor Darkened {
      get {
        return new HtmlColor((byte)(R * 0.3), (byte)(G * 0.3), (byte)(B * 0.3));
      }
    }

    #endregion

    #region predefined colors

    /// <summary>
    /// Black color.
    /// </summary>
    public static HtmlColor Black {
      get { return new HtmlColor(0, 0, 0); }
    }

    /// <summary>
    /// White color.
    /// </summary>
    public static HtmlColor White {
      get { return new HtmlColor(255, 255, 255); }
    }

    /// <summary>
    /// Red color.
    /// </summary>
    public static HtmlColor Red {
      get { return new HtmlColor(255, 0, 0); }
    }

    /// <summary>
    /// Green color.
    /// </summary>
    public static HtmlColor Green {
      get { return new HtmlColor(0, 255, 0); }
    }

    /// <summary>
    /// Blue color.
    /// </summary>
    public static HtmlColor Blue {
      get { return new HtmlColor(0, 0, 255); }
    }

    /// <summary>
    /// Try to parse a color string (without leading "#").
    /// </summary>
    /// <param name="color">The string to parse</param>
    /// <returns>A valid color representation or <see cref="HtmlColor.Black" /> if the string could not be parsed</returns>
    public static HtmlColor Parse(string color) {
      Regex re = new Regex(@"^([0-9a-zA-Z][0-9a-zA-Z])([0-9a-zA-Z][0-9a-zA-Z])([0-9a-zA-Z][0-9a-zA-Z])$");
      if (re.IsMatch(color)) {
        try {
          Match m = re.Match(color);
          byte red = byte.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
          byte green = byte.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
          byte blue = byte.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
          return new HtmlColor(red, green, blue);
        }
        catch {
          return Black;
        }
      }
      else {
        return Black;
      }
    }

    #endregion

    /// <summary>
    /// Use the given parameters for this HTML color.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    public void FromRGB(byte r, byte g, byte b) {
      _R = r;
      _G = g;
      _B = b;
    }

    /// <summary>
    /// Convert this HTML-Color into a .NET color.
    /// </summary>
    /// <returns>The .NET color</returns>
    public System.Drawing.Color ToNetColor() {
      return System.Drawing.Color.FromArgb(_R, _G, _B);
    }

  }

}
