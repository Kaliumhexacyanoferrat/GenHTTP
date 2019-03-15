using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Style
{

    /// <summary>
    /// Abstraction of a CSS font.
    /// </summary>
    /// <remarks>
    /// The abstract font pool needs to be extended.
    /// </remarks>
    public class ElementFont
    {
        private ElementSize _Size;
        private string _Name;

        #region Constructors

        /// <summary>
        /// Create a new, empty font object.
        /// </summary>
        public ElementFont()
        {
            _Size = new ElementSize();
        }

        /// <summary>
        /// Create a new font object.
        /// </summary>
        /// <param name="font">The name of the font</param>
        /// <remarks>Default font size is 10pt</remarks>
        public ElementFont(string font)
        {
            _Name = font;
            _Size = new ElementSize(10, ElementSizeType.Pt);
        }

        /// <summary>
        /// Create a new font object.
        /// </summary>
        /// <param name="size">The size of the font</param>
        public ElementFont(ushort size)
        {
            _Size = new ElementSize(size, ElementSizeType.Pt);
        }

        /// <summary>
        /// Create a new font object.
        /// </summary>
        /// <param name="font">The name of the font</param>
        /// <param name="size">The size of the font in points</param>
        public ElementFont(string font, ushort size)
        {
            _Name = font;
            _Size = new ElementSize(size, ElementSizeType.Pt);
        }

        /// <summary>
        /// Create a new font object.
        /// </summary>
        /// <param name="font">The name of the font</param>
        /// <param name="size">The font's size</param>
        /// <param name="sizeType">The type of the font's size</param>
        public ElementFont(string font, ushort size, ElementSizeType sizeType)
        {
            _Name = font;
            _Size = new ElementSize(size, sizeType);
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// The size of the font.
        /// </summary>
        public ElementSize Size
        {
            get { return _Size; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Retrieve the string representation of this element.
        /// </summary>
        /// <returns>The CSS string</returns>
        internal string ToCss()
        {
            string ret = "";
            if (_Name != null && _Name.Length > 0)
            {
                ret += " font-family: " + _Name + ";";
            }
            string size = _Size.ToCss();
            if (size.Length > 0)
            {
                ret += " font-size: " + _Size.ToCss() + ";";
            }
            return ret;
        }

        #endregion

        #region Pre-defined fonts

        /// <summary>
        /// Font 'Arial'.
        /// </summary>
        public static ElementFont Arial
        {
            get { return new ElementFont("Arial"); }
        }

        /// <summary>
        /// Font 'Tahoma'.
        /// </summary>
        public static ElementFont Tahoma
        {
            get { return new ElementFont("Tahoma"); }
        }

        /// <summary>
        /// Font 'Verdana'.
        /// </summary>
        public static ElementFont Verdana
        {
            get { return new ElementFont("Verdana"); }
        }

        /// <summary>
        /// Font 'Calibri'.
        /// </summary>
        public static ElementFont Calibri
        {
            get { return new ElementFont("Calibri"); }
        }

        /// <summary>
        /// Font 'Batang'.
        /// </summary>
        public static ElementFont Batang
        {
            get { return new ElementFont("Batang"); }
        }

        /// <summary>
        /// Font 'Bookman Old Style'.
        /// </summary>
        public static ElementFont BookmanOldStlye
        {
            get { return new ElementFont("Bookman Old Style"); }
        }

        /// <summary>
        /// Font 'Cambria'.
        /// </summary>
        public static ElementFont Cambria
        {
            get { return new ElementFont("Cambria"); }
        }

        /// <summary>
        /// Font 'Comic Sans MS'.
        /// </summary>
        public static ElementFont ComicSansMS
        {
            get { return new ElementFont("Comic Sans MS"); }
        }

        /// <summary>
        /// Font 'Georgia'.
        /// </summary>
        public static ElementFont Georgia
        {
            get { return new ElementFont("Georgia"); }
        }

        /// <summary>
        /// Font 'Helvetica'.
        /// </summary>
        public static ElementFont Helvetica
        {
            get { return new ElementFont("Helvetica"); }
        }

        /// <summary>
        /// Font 'Kartika'.
        /// </summary>
        public static ElementFont Kartika
        {
            get { return new ElementFont("Kartika"); }
        }

        /// <summary>
        /// Font 'Times New Roman'.
        /// </summary>
        public static ElementFont TimesNewRoman
        {
            get { return new ElementFont("Times New Roman"); }
        }

        #endregion

    }

}
