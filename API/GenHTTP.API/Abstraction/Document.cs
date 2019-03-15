using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;

using GenHTTP.Api.Abstraction.Style;
using GenHTTP.Api.Abstraction.Elements;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// This class defines a document, which can be handled
    /// by the GenHTTP library.
    /// </summary>
    /// <remarks>
    /// This class does not support all features of the W3C
    /// standards:
    /// 
    /// - You can't give the 'html' tag an ID
    /// </remarks>
    public class Document
    {
        private DocumentHeader _Header;
        private DocumentBody _Body;
        private DocumentEncoding _Encoding;
        private DocumentType _Type;
        private Internationalization _Internationalization;
        private int _EstimatedSize = 2000;

        #region constructors

        /// <summary>
        /// Create a new document of a pre-defined type.
        /// </summary>
        /// <param name="type">The type of this document</param>
        public Document(DocumentType type) : this()
        {
            _Type = type;
        }

        /// <summary>
        /// Create a new, empty document.
        /// </summary>
        public Document()
        {
            _Encoding = new DocumentEncoding();
            _Type = DocumentType.XHtml_1_1_Strict;
            _Internationalization = new Internationalization();
            _Header = new DocumentHeader(this);
            _Body = new DocumentBody();
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The header of this document.
        /// </summary>
        public DocumentHeader Header
        {
            get
            {
                return _Header;
            }
        }

        /// <summary>
        /// The body of this document.
        /// </summary>
        public DocumentBody Body
        {
            get
            {
                return _Body;
            }
        }

        /// <summary>
        /// The encoding of this document.
        /// </summary>
        public DocumentEncoding Encoding
        {
            get { return _Encoding; }
        }

        /// <summary>
        /// Set properties like the language or the text
        /// direction for this document.
        /// </summary>
        public Internationalization Internationalization
        {
            get { return _Internationalization; }
        }

        /// <summary>
        /// Check, whether this document will generate
        /// XHTML code or not.
        /// </summary>
        internal bool IsXHtml
        {
            get { return _Type == DocumentType.XHtml_1_1_Strict || _Type == DocumentType.XHtml_1_1_Transitional; }
        }

        /// <summary>
        /// Check, whether this document will generate
        /// strict (X)HTML code or not.
        /// </summary>
        internal bool IsStrict
        {
            get { return _Type == DocumentType.Html_4_01_Strict || _Type == DocumentType.XHtml_1_1_Strict; }
        }

        /// <summary>
        /// The estimated size of this page in characters.
        /// </summary>
        /// <remarks>
        /// This property describes the initial capacity of the
        /// <see cref="StringBuilder" /> used to generate the document.
        /// Setting this property to a proper size may improve the generation
        /// performance. Value may be not 0 or lesser.
        /// </remarks>
        public int EstimatedSize
        {
            get { return _EstimatedSize; }
            set { _EstimatedSize = (value > 0) ? value : 2000; }
        }

        /// <summary>
        /// Get or set the type of this document.
        /// </summary>
        public DocumentType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this document to (X)HTML code.
        /// </summary>
        /// <returns>The encoded representation of this document</returns>
        public byte[] Serialize()
        {
            // encode the generated text with the requested encoding
            return _Encoding.Encoding.GetBytes(SerializeToString());
        }

        /// <summary>
        /// Serialize this document to (X)HTML code.
        /// </summary>
        /// <returns>The string representation of this document</returns>
        public string SerializeToString()
        {
            // create the string builder to write to
            StringBuilder b = new StringBuilder(_EstimatedSize);
            // write XML tag
            b.Append(@"<?xml version=""1.0"" encoding=""" + _Encoding.Name + @"""?>" + "\r\n");
            // write doctype
            switch (_Type)
            {
                case DocumentType.Html_4_01_Strict:
                    b.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN""" + "\r\n" + @"""http://www.w3.org/TR/html4/strict.dtd"">");
                    break;
                case DocumentType.Html_4_01_Transitional:
                    b.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""" + "\r\n" + @"""http://www.w3.org/TR/html4/loose.dtd"">");
                    break;
                case DocumentType.XHtml_1_1_Strict:
                    b.Append(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN""" + "\r\n" + @"""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
                    break;
                case DocumentType.XHtml_1_1_Transitional:
                    b.Append(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN""" + "\r\n" + @"""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">");
                    break;
            }
            // write html tag
            b.Append("\r\n\r\n");
            b.Append("<html");
            // XML namespace
            if (IsXHtml) b.Append(@" xmlns=""http://www.w3.org/1999/xhtml""");
            // serialize internationalization settings
            _Internationalization.Serialize(b, IsXHtml);
            // finish html tag
            b.Append(">\r\n\r\n");
            // serialize header
            _Header.Serialize(b);
            // serialize body
            _Body.Serialize(b, _Type);
            // close html tag
            b.Append("</html>");
            // convert the result into a string
            return b.ToString();
        }

        #endregion

    }

}
