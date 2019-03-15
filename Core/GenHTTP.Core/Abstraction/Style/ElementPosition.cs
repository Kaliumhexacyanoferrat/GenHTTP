using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Style
{

    /// <summary>
    /// The relative or absoulte position of an element.
    /// </summary>
    public class ElementPosition
    {
        private ElementSize _Left;
        private ElementSize _Right;
        private ElementSize _Top;
        private ElementSize _Bottom;
        private ElementSize _Value;

        #region Constructors

        /// <summary>
        /// Create a new position object.
        /// </summary>
        public ElementPosition()
        {

        }

        /// <summary>
        /// Create a new position object.
        /// </summary>
        /// <param name="value">The value of the position in pixels</param>
        public ElementPosition(ushort value)
        {
            _Value = new ElementSize(value);
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// Left.
        /// </summary>
        public ElementSize Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        /// <summary>
        /// Right.
        /// </summary>
        public ElementSize Right
        {
            get { return _Right; }
            set { _Right = value; }
        }

        /// <summary>
        /// Top.
        /// </summary>
        public ElementSize Top
        {
            get { return _Top; }
            set { _Top = value; }
        }

        /// <summary>
        /// Bottom.
        /// </summary>
        public ElementSize Bottom
        {
            get { return _Bottom; }
            set { _Bottom = value; }
        }

        /// <summary>
        /// Value.
        /// </summary>
        public ElementSize Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize to CSS.
        /// </summary>
        /// <param name="attrib">The name of the CSS attribute</param>
        /// <returns>The string representation</returns>
        public string ToCss(string attrib)
        {
            string ret = "";
            string css = (_Value != null) ? _Value.ToCss() : "";
            if (css.Length > 0) ret += " " + attrib + ": " + css + ";";
            css = (_Left != null) ? _Left.ToCss() : "";
            if (css.Length > 0) ret += " " + attrib + "-left: " + css + ";";
            css = (_Right != null) ? _Right.ToCss() : "";
            if (css.Length > 0) ret += " " + attrib + "-right: " + css + ";";
            css = (_Top != null) ? _Top.ToCss() : "";
            if (css.Length > 0) ret += " " + attrib + "-top: " + css + ";";
            css = (_Bottom != null) ? _Bottom.ToCss() : "";
            if (css.Length > 0) ret += " " + attrib + "-bottom: " + css + ";";
            return ret;
        }

        #endregion

    }

}
