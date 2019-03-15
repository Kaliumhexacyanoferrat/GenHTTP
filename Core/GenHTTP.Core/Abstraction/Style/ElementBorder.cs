using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Style
{

    /// <summary>
    /// Represents a CSS border.
    /// </summary>
    public class ElementBorder
    {
        private ElementSize _Size;
        private ElementColor _Color;
        private ElementBorderType _Type = ElementBorderType.Solid;

        #region Constructors

        /// <summary>
        /// Create a new element border (1px, solid, black).
        /// </summary>
        public ElementBorder()
        {
            Size = new ElementSize(1);
            Color = ElementColor.Black;
        }

        /// <summary>
        /// Create a new element border (solid, black).
        /// </summary>
        /// <param name="size">The size of the border (in pixels)</param>
        public ElementBorder(ushort size)
        {
            Size = new ElementSize(size);
            Color = ElementColor.Black;
        }

        /// <summary>
        /// Create a new element border (solid).
        /// </summary>
        /// <param name="size">The size of the border in pixels</param>
        /// <param name="color">The color of the border</param>
        public ElementBorder(ushort size, ElementColor color)
        {
            Size = new ElementSize(size);
            Color = color;
        }

        /// <summary>
        /// Create a new element border (black).
        /// </summary>
        /// <param name="size">The size of the border in pixels</param>
        /// <param name="type">The type of the border</param>
        public ElementBorder(ushort size, ElementBorderType type)
        {
            Size = new ElementSize(size);
            Type = type;
            Color = ElementColor.Black;
        }

        /// <summary>
        /// Create a new element border (1px).
        /// </summary>
        /// <param name="color">The color of the border</param>
        /// <param name="type">The type of the border</param>
        public ElementBorder(ElementColor color, ElementBorderType type)
        {
            Size = new ElementSize(1);
            Color = color;
            Type = type;
        }

        /// <summary>
        /// Create a new element border (1px, solid).
        /// </summary>
        /// <param name="color">The color of the border</param>
        public ElementBorder(ElementColor color)
        {
            Size = new ElementSize(1);
            Color = color;
        }

        /// <summary>
        /// Create a new element border (1px, black).
        /// </summary>
        /// <param name="type">The type of the border</param>
        public ElementBorder(ElementBorderType type)
        {
            Size = new ElementSize(1);
            Color = ElementColor.Black;
            Type = type;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The size of the border (in pixels).
        /// </summary>
        public ElementSize Size
        {
            get { return _Size; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _Size = value;
            }
        }

        /// <summary>
        /// The color of the border.
        /// </summary>
        public ElementColor Color
        {
            get { return _Color; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _Color = value;
            }
        }

        /// <summary>
        /// The type of the border.
        /// </summary>
        public ElementBorderType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize to CSS.
        /// </summary>
        /// <returns>The CSS representation of this border element</returns>
        public string ToCss()
        {
            return " border: " + _Size.ToCss() + " " + GetBorderTypeName(_Type) + " " + _Color.ToCss() + ";";
        }

        /// <summary>
        /// Get the name of a border type.
        /// </summary>
        /// <param name="type">The border to convert</param>
        /// <returns>The converted string</returns>
        public static string GetBorderTypeName(ElementBorderType type)
        {
            string name = type.ToString();
            return name.ToLower()[0] + name.Substring(1);
        }

        #endregion

    }

}
