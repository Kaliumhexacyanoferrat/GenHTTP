using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// An element which allows you to add plain text to
    /// another element.
    /// </summary>
    public class Text : Element
    {
        private string _Value;
        private bool _EscapeEntities = true;

        #region Constructors

        /// <summary>
        /// Create a new, empty text element.
        /// </summary>
        public Text()
        {

        }

        /// <summary>
        /// Create a new text element.
        /// </summary>
        /// <param name="value">The value of the element</param>
        public Text(string value)
        {
            _Value = value;
        }

        /// <summary>
        /// Create a new text element.
        /// </summary>
        /// <param name="value">The value of the element</param>
        /// <param name="escapeEntities">Define, whether HTML entities should be escaped or not</param>
        /// <remarks>
        /// By default, this class will escape all HTML entities.
        /// </remarks>
        public Text(string value, bool escapeEntities)
        {
            _Value = value;
            _EscapeEntities = escapeEntities;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The value of this text element.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// Define, whether HTML entities should be escaped.
        /// </summary>
        /// <remarks>
        /// By default, this property is set to true.
        /// </remarks>
        public bool EscapeEntities
        {
            get { return _EscapeEntities; }
            set { _EscapeEntities = value; }
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
            if (_Value != null && _Value.Length > 0)
            {
                b.Append((EscapeEntities) ? DocumentEncoding.ConvertString(_Value) : _Value);
            }
        }

        #endregion

    }

}
