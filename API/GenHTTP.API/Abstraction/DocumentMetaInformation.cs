using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// Objects of this class allow you to define
    /// meta data for a <see cref="Document"/>.
    /// </summary>
    /// <remarks>
    /// It's recommended to use the methods and properties of the <see cref="HttpResponse"/>
    /// instead of adding meta data using the <see cref="DocumentMetaInformationType.HttpEquivalent" />
    /// type.
    /// 
    /// The meta information data structure does not support IDs.
    /// </remarks>
    public class DocumentMetaInformation
    {
        private string _Name;
        private DocumentMetaInformationType _Type = DocumentMetaInformationType.Normal;
        private string _Content;
        private string _Scheme;
        private Internationalization _Internationalization;

        #region constructors

        /// <summary>
        /// Add meta information.
        /// </summary>
        /// <param name="content">The content of the meta field</param>
        public DocumentMetaInformation(string content)
        {
            Content = content;
            _Internationalization = new Internationalization();
        }

        /// <summary>
        /// Add named meta information.
        /// </summary>
        /// <param name="name">The name of the meta field</param>
        /// <param name="content">The content of the meta field</param>
        public DocumentMetaInformation(string name, string content) : this(content)
        {
            _Name = name;
        }

        /// <summary>
        /// Add named meta information.
        /// </summary>
        /// <param name="name">The name of the meta field</param>
        /// <param name="scheme">The scheme to use for this meta field</param>
        /// <param name="content">The content of the meta field</param>
        public DocumentMetaInformation(string name, string scheme, string content) : this(name, content)
        {
            _Scheme = scheme;
        }

        /// <summary>
        /// Add meta information.
        /// </summary>
        /// <param name="name">The name of the meta field or HTTP equivalent</param>
        /// <param name="type">The type of the meta tag</param>
        /// <param name="content">The content of the meta field</param>
        public DocumentMetaInformation(string name, DocumentMetaInformationType type, string content) : this(name, content)
        {
            _Type = type;
        }

        /// <summary>
        /// Add meta information.
        /// </summary>
        /// <param name="name">The name of the meta field or HTTP equivalent</param>
        /// <param name="type">The type of the meta tag</param>
        /// <param name="scheme">The scheme to use for this meta field</param>
        /// <param name="content">The content of the meta field</param>
        public DocumentMetaInformation(string name, DocumentMetaInformationType type, string scheme, string content) : this(name, scheme, content)
        {
            _Type = type;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The name of the meta tag.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// The content of the meta tag.
        /// </summary>
        public string Content
        {
            get { return _Content; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length == 0) throw new ArgumentException("The content of the meta tag can't be empty", "value");
                _Content = value;
            }
        }

        /// <summary>
        /// The type of this meta tag.
        /// </summary>
        public DocumentMetaInformationType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// The scheme of this meta tag.
        /// </summary>
        public string Scheme
        {
            get { return _Scheme; }
            set { _Scheme = value; }
        }

        /// <summary>
        /// Set the language of this meta tag.
        /// </summary>
        public Internationalization Internationalization
        {
            get { return _Internationalization; }
            set { _Internationalization = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this meta tag.
        /// </summary>
        /// <param name="b">The string builder to serialize to</param>
        /// <param name="doc">The document related to this meta tag</param>
        internal void Serialize(StringBuilder b, Document doc)
        {
            string tagEnd = (doc.IsXHtml) ? " />" : ">";
            string offset = "  ";
            string tagID = (_Type == DocumentMetaInformationType.Normal) ? "name" : "http-equiv";
            b.Append(offset + "<meta");
            if (_Name != null && _Name.Length > 0)
            {
                b.Append(" " + tagID + "=\"" + DocumentEncoding.ConvertString(_Name) + "\"");
            }
            _Internationalization.Serialize(b, doc.IsXHtml);
            if (_Scheme != null && _Scheme.Length > 0)
            {
                b.Append(" scheme=\"" + DocumentEncoding.ConvertString(_Scheme) + "\"");
            }
            b.Append(" content=\"" + DocumentEncoding.ConvertString(_Content) + "\"" + tagEnd + "\r\n");
        }

        #endregion

    }

}
