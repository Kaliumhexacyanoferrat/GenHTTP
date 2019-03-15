using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// Defines a headline.
    /// </summary>
    public class Headline : StyledElement
    {
        private string _Value;
        private byte _Size = 1;

        #region Constructors

        /// <summary>
        /// Create a new headline using the largest font.
        /// </summary>
        /// <param name="value">The value of this headling</param>
        public Headline(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a new headling with a specific size.
        /// </summary>
        /// <param name="value">The value of this headline</param>
        /// <param name="size">The size of this headline (from 1 to 6)</param>
        public Headline(string value, byte size)
        {
            Value = value;
            Size = size;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The value of the headline.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set
            {
                if (value == null || value.Length == 0) throw new ArgumentException("The value of a headline cannot be null or empty");
                _Value = value;
            }
        }

        /// <summary>
        /// The size of the headline (from 1 to 6).
        /// </summary>
        public byte Size
        {
            get { return _Size; }
            set
            {
                if (value > 6) throw new ArgumentException("The size of a headline cannot be smaller than 6");
                _Size = value;
            }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            RenderingType = type;
            b.Append("\r\n\r\n<h" + _Size + ">" + DocumentEncoding.ConvertString(_Value) + "</h" + _Size + ">\r\n");
        }

        #endregion

    }

}
