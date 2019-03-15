
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GenHTTP.Abstraction.Style
{

    /// <summary>
    /// Represents a CSS color.
    /// </summary>
    public class ElementColor
    {
        private string _Color;

        #region Constructors

        /// <summary>
        /// Create a new color object from a color string.
        /// </summary>
        /// <param name="color">The color string</param>
        public ElementColor(string color)
        {
            if (color == null) throw new ArgumentNullException();
            _Color = color;
        }

        /// <summary>
        /// Create a new color object from a .NET color.
        /// </summary>
        /// <param name="color">The color to use</param>
        public ElementColor(Color color)
        {
            if (color == null) throw new ArgumentNullException();
            _Color = "#" + FillLeadingZero(color.R) + FillLeadingZero(color.G) + FillLeadingZero(color.B);
        }

        /// <summary>
        /// Create a new color object from a RGB code.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public ElementColor(int r, int g, int b)
        {
            _Color = "#" + FillLeadingZero(r) + FillLeadingZero(g) + FillLeadingZero(b);
        }

        private string FillLeadingZero(int code)
        {
            string hex = code.ToString("X");
            return (hex.Length == 1) ? "0" + hex : hex;
        }

        #endregion

        #region Pre-defined colors

        /// <summary>
        /// Aquamarine.
        /// </summary>
        public static ElementColor Aqua
        {
            get { return new ElementColor("aqua"); }
        }

        /// <summary>
        /// Black.
        /// </summary>
        public static ElementColor Black
        {
            get { return new ElementColor("black"); }
        }

        /// <summary>
        /// Blue.
        /// </summary>
        public static ElementColor Blue
        {
            get { return new ElementColor("blue"); }
        }

        /// <summary>
        /// Fuchsia.
        /// </summary>
        public static ElementColor Fuchsia
        {
            get { return new ElementColor("fuchsia"); }
        }

        /// <summary>
        /// Gray.
        /// </summary>
        public static ElementColor Gray
        {
            get { return new ElementColor("gray"); }
        }

        /// <summary>
        /// Green.
        /// </summary>
        public static ElementColor Green
        {
            get { return new ElementColor("green"); }
        }

        /// <summary>
        /// Lime.
        /// </summary>
        public static ElementColor Lime
        {
            get { return new ElementColor("lime"); }
        }

        /// <summary>
        /// Maroon.
        /// </summary>
        public static ElementColor Maroon
        {
            get { return new ElementColor("maroon"); }
        }

        /// <summary>
        /// Navy.
        /// </summary>
        public static ElementColor Navy
        {
            get { return new ElementColor("navy"); }
        }

        /// <summary>
        /// Olive.
        /// </summary>
        public static ElementColor Olive
        {
            get { return new ElementColor("olive"); }
        }

        /// <summary>
        /// Purple.
        /// </summary>
        public static ElementColor Purple
        {
            get { return new ElementColor("purple"); }
        }

        /// <summary>
        /// Red.
        /// </summary>
        public static ElementColor Red
        {
            get { return new ElementColor("red"); }
        }

        /// <summary>
        /// Silver.
        /// </summary>
        public static ElementColor Silver
        {
            get { return new ElementColor("silver"); }
        }

        /// <summary>
        /// Teal.
        /// </summary>
        public static ElementColor Teal
        {
            get { return new ElementColor("teal"); }
        }

        /// <summary>
        /// White.
        /// </summary>
        public static ElementColor White
        {
            get { return new ElementColor("white"); }
        }

        /// <summary>
        /// Yellow.
        /// </summary>
        public static ElementColor Yellow
        {
            get { return new ElementColor("yellow"); }
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The CSS color string.
        /// </summary>
        public string Value
        {
            get { return _Color; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Retrieve the CSS representation of this color.
        /// </summary>
        /// <returns>The CSS representation of the color</returns>
        public string ToCss()
        {
            return _Color;
        }

        #endregion

    }

}
