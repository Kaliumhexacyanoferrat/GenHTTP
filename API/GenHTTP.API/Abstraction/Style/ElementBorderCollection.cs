using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Style
{

    /// <summary>
    /// Allows you to add borders to an element.
    /// </summary>
    /// <remarks>
    /// Border-handling is really simplified.
    /// </remarks>
    public class ElementBorderCollection
    {
        private ElementBorder _Left, _Right, _Top, _Bottom, _Value;

        #region Constructors

        /// <summary>
        /// Create a new element border (solid, black).
        /// </summary>
        /// <param name="size">The size of the border (in pixels)</param>
        public ElementBorderCollection(ushort size)
        {
            _Value = new ElementBorder(size);
        }

        /// <summary>
        /// Create a new element border (solid).
        /// </summary>
        /// <param name="size">The size of the border in pixels</param>
        /// <param name="color">The color of the border</param>
        public ElementBorderCollection(ushort size, ElementColor color)
        {
            _Value = new ElementBorder(size, color);
        }

        /// <summary>
        /// Create a new element border (black).
        /// </summary>
        /// <param name="size">The size of the border in pixels</param>
        /// <param name="type">The type of the border</param>
        public ElementBorderCollection(ushort size, ElementBorderType type)
        {
            _Value = new ElementBorder(size, type);
        }

        /// <summary>
        /// Create a new element border (1px).
        /// </summary>
        /// <param name="color">The color of the border</param>
        /// <param name="type">The type of the border</param>
        public ElementBorderCollection(ElementColor color, ElementBorderType type)
        {
            _Value = new ElementBorder(color, type);
        }

        /// <summary>
        /// Create a new element border (1px, solid).
        /// </summary>
        /// <param name="color">The color of the border</param>
        public ElementBorderCollection(ElementColor color)
        {
            _Value = new ElementBorder(color);
        }

        /// <summary>
        /// Create a new element border (1px, black).
        /// </summary>
        /// <param name="type">The type of the border</param>
        public ElementBorderCollection(ElementBorderType type)
        {
            _Value = new ElementBorder(type);
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The left border of the element.
        /// </summary>
        public ElementBorder Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        /// <summary>
        /// The right border of the element.
        /// </summary>
        public ElementBorder Right
        {
            get { return _Right; }
            set { _Right = value; }
        }

        /// <summary>
        /// The upper border of the element.
        /// </summary>
        public ElementBorder Top
        {
            get { return _Top; }
            set { _Top = value; }
        }

        /// <summary>
        /// The lower border of the element.
        /// </summary>
        public ElementBorder Bottom
        {
            get { return _Bottom; }
            set { _Bottom = value; }
        }

        /// <summary>
        /// The border of the element.
        /// </summary>
        public ElementBorder Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize to CSS.
        /// </summary>
        /// <returns>The CSS representation of the border</returns>
        public string ToCss()
        {
            string ret = "";
            if (_Value != null) ret += _Value.ToCss();
            if (_Left != null) ret += _Left.ToCss();
            if (_Right != null) ret += _Right.ToCss();
            if (_Top != null) ret += _Top.ToCss();
            if (_Bottom != null) ret += _Bottom.ToCss();
            return ret;
        }

        #endregion

    }

}
