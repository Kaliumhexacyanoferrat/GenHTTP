using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// A hyperlink.
    /// </summary>
    public class Link : StyledContainerElement
    {
        private string _URL;

        #region Constructors

        /// <summary>
        /// Create a new, empty link.
        /// </summary>
        public Link()
        {

        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        public Link(string url)
        {
            _URL = url;
        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <param name="text">The link text</param>
        public Link(string url, string text) : this(url)
        {
            _Children.Add(new Text(text));
        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <param name="element">The link element</param>
        public Link(string url, Element element) : this(url)
        {
            _Children.Add(element);
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The URL of this link.
        /// </summary>
        public string Url
        {
            get { return _URL; }
            set { _URL = value; }
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
            b.Append("<a");
            if (_URL != null && _URL.Length > 0) b.Append(" href=\"" + _URL + "\"");
            b.Append(ToXHtml(IsXHtml) + ToClassString() + ToCss() + ">");
            SerializeChildren(b, type);
            b.Append("</a>");
        }

        #endregion

    }

}
