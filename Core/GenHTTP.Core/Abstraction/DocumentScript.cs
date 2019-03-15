using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// A script or a reference to a script.
    /// </summary>
    public class DocumentScript
    {
        private ContentType _Type = ContentType.ApplicationJavaScript;
        private string _Source;
        private DocumentEncoding _Charset;
        private bool _Defer;
        private string _Value;

        #region Constructors

        /// <summary>
        /// Create a new document script.
        /// </summary>
        /// <param name="source">The URL of the script</param>
        public DocumentScript(string source)
        {
            _Source = source;
        }

        /// <summary>
        /// Create a new document script.
        /// </summary>
        /// <param name="source">The URL of the script</param>
        /// <param name="type">The type of the script</param>
        public DocumentScript(string source, ContentType type)
        {
            _Source = source;
            _Type = type;
        }

        /// <summary>
        /// Create a new document script.
        /// </summary>
        /// <param name="type">The script type</param>
        /// <param name="value">The code of the script</param>
        public DocumentScript(ContentType type, string value)
        {
            _Type = type;
            _Value = value;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The type of the script.
        /// </summary>
        /// <remarks>
        /// Required property. The value of this
        /// field is set to ApplicationJavaScript by default.
        /// </remarks>
        public ContentType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// If you want to reference an external script, set
        /// this value to the URL of this script.
        /// </summary>
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        /// <summary>
        /// Define the charset of the script.
        /// </summary>
        /// <remarks>
        /// It's not required to set this field.
        /// </remarks>
        public DocumentEncoding Charset
        {
            get { return _Charset; }
            set { _Charset = value; }
        }

        /// <summary>
        /// If you set this value to true, the user agent
        /// won't wait for the script to load.
        /// </summary>
        /// <remarks>
        /// This property is only supported by a very small number
        /// of user agents.
        /// </remarks>
        public bool Defer
        {
            get { return _Defer; }
            set { _Defer = value; }
        }

        /// <summary>
        /// Set or get the code of the script.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this script information.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="doc">The related document</param>
        /// <param name="offset">The offset to add before the tag</param>
        public void Serialize(StringBuilder b, Document doc, string offset)
        {
            b.Append(offset);
            b.Append("<script type=\"" + HttpResponseHeader.GetContentType(_Type) + "\"");
            if (_Charset != null)
            {
                b.Append(" charset=\"" + _Charset.Name + "\"");
            }
            if (_Defer)
            {
                if (!doc.IsXHtml)
                {
                    b.Append(" defer");
                }
                else
                {
                    b.Append(" defer=\"defer\"");
                }
            }
            if (_Source != null && _Source.Length > 0)
            {
                b.Append(" src=\"" + _Source + "\"");
            }
            if (_Value != null && _Value.Length > 0)
            {
                b.Append(">\r\n");
                b.Append(_Value);
                b.Append("\r\n" + offset + "</script>");
            }
            else
            {
                b.Append("></script>");
            }
            b.Append("\r\n");
        }

        #endregion

    }

}
