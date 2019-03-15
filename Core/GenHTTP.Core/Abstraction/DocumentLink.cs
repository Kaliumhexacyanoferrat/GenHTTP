using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// The abstraction of a link to another document.
    /// </summary>
    /// <remarks>
    /// This implementation does not provide the core attributes
    /// 'id', 'class' and 'style'.
    /// </remarks>
    public class DocumentLink
    {
        private string _HypertextReference;
        private string _Title;
        private ContentType _Type;
        private bool _UseType = false;
        private LinkType _Rel = LinkType.Unspecified;
        private string _UserDefinedRel;
        private LinkType _Rev = LinkType.Unspecified;
        private string _UserDefinedRev;
        private MediaType _Media = MediaType.Unspecified;
        private LanguageInfo _HypertextReferenceLanguage;
        private DocumentEncoding _Charset;
        private Internationalization _Internationalization;

        #region Constructors

        /// <summary>
        /// Create a new, empty link.
        /// </summary>
        public DocumentLink()
        {
            _Internationalization = new Internationalization();
        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        /// <param name="rel">The type of the link</param>
        /// <param name="hypertextReference">The URL to link to</param>
        public DocumentLink(LinkType rel, string hypertextReference) : this()
        {
            Rel = rel;
            HypertextReference = hypertextReference;
        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        /// <param name="rel">The type of the link</param>
        /// <param name="type">The content type of the linked document</param>
        /// <param name="hypertextReference">The URL to link to</param>
        public DocumentLink(LinkType rel, ContentType type, string hypertextReference) : this()
        {
            Rel = rel;
            Type = type;
            HypertextReference = hypertextReference;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The URL to link to.
        /// </summary>
        public string HypertextReference
        {
            get { return _HypertextReference; }
            set { _HypertextReference = value; }
        }

        /// <summary>
        /// Same as <see cref="HypertextReference" />.
        /// </summary>
        public string Href
        {
            get { return HypertextReference; }
            set { HypertextReference = value; }
        }

        /// <summary>
        /// The language of the linked document.
        /// </summary>
        public LanguageInfo HypertextReferenceLanguage
        {
            get { return _HypertextReferenceLanguage; }
            set { _HypertextReferenceLanguage = value; }
        }

        /// <summary>
        /// The content type of the linked document.
        /// </summary>
        /// <remarks>
        /// You need to set this value explicitely to
        /// enable the ouput of the content type.
        /// </remarks>
        public ContentType Type
        {
            get { return _Type; }
            set { _Type = value; _UseType = true; }
        }

        /// <summary>
        /// Forward link type.
        /// </summary>
        public LinkType Rel
        {
            get { return _Rel; }
            set { _Rel = value; }
        }

        /// <summary>
        /// User-defined link type.
        /// </summary>
        /// <remarks>
        /// Set Rel to LinkType.Other if you want
        /// to use an user-defined link type.
        /// </remarks>
        public string UserDefinedRel
        {
            get { return _UserDefinedRel; }
            set { _UserDefinedRel = value; }
        }

        /// <summary>
        /// Reverse link type.
        /// </summary>
        public LinkType Rev
        {
            get { return _Rev; }
            set { _Rev = value; }
        }

        /// <summary>
        /// User-defined link type.
        /// </summary>
        /// <remarks>
        /// Set Rev to LinkType.Other if you want
        /// to use an user-defined link type.
        /// </remarks>
        public string UserDefinedRev
        {
            get { return _UserDefinedRev; }
            set { _UserDefinedRev = value; }
        }

        /// <summary>
        /// The media used for this document.
        /// </summary>
        public MediaType Media
        {
            get { return _Media; }
            set { _Media = value; }
        }

        /// <summary>
        /// Language and text direction.
        /// </summary>
        public Internationalization Internationalization
        {
            get { return _Internationalization; }
            set { _Internationalization = value; }
        }

        /// <summary>
        /// The charset of the linked document.
        /// </summary>
        public DocumentEncoding Charset
        {
            get { return _Charset; }
            set { _Charset = value; }
        }

        /// <summary>
        /// The title of this link.
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this element to (X)HTML.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="doc">The related document</param>
        internal void Serialize(StringBuilder b, Document doc)
        {
            // write offset
            b.Append("  ");
            // write tag
            b.Append("<link");
            // rel & rev
            if (_Rel != LinkType.Unspecified)
            {
                if (_Rel != LinkType.Other)
                {
                    b.Append(" rel=\"" + _Rel.ToString().ToLower() + "\"");
                }
                else
                {
                    if (_UserDefinedRel != null && _UserDefinedRel.Length > 0) b.Append(" rel=\"" + _UserDefinedRel + "\"");
                }
            }
            if (_Rev != LinkType.Unspecified)
            {
                if (_Rev != LinkType.Other)
                {
                    b.Append(" rev=\"" + _Rev.ToString().ToLower() + "\"");
                }
                else
                {
                    if (_UserDefinedRev != null && _UserDefinedRev.Length > 0) b.Append(" rev=\"" + _UserDefinedRel + "\"");
                }
            }
            // title
            if (_Title != null && _Title.Length > 0) b.Append(" title=\"" + DocumentEncoding.ConvertString(_Title) + "\"");
            // type
            if (_UseType)
            {
                b.Append(" type=\"" + HttpResponseHeader.GetContentType(_Type) + "\"");
            }
            // localization
            _Internationalization.Serialize(b, doc.IsXHtml);
            // charset
            if (_Charset != null)
            {
                b.Append(" charset=\"" + _Charset.Name + "\"");
            }
            // language of the document
            if (_HypertextReferenceLanguage != null)
            {
                b.Append(" hreflang=\"" + _HypertextReferenceLanguage.LanguageString + "\"");
            }
            // media type
            if (_Media != MediaType.Unspecified)
            {
                string mediaString = "";
                if (_Media == MediaType.All) mediaString += ", all";
                if (_Media == MediaType.Aural) mediaString += ", aural";
                if (_Media == MediaType.Braille) mediaString += ", braille";
                if (_Media == MediaType.Handheld) mediaString += ", handheld";
                if (_Media == MediaType.Print) mediaString += ", print";
                if (_Media == MediaType.Projection) mediaString += ", projection";
                if (_Media == MediaType.Screen) mediaString += ", screen";
                if (_Media == MediaType.TTY) mediaString += ", tty";
                if (_Media == MediaType.TV) mediaString += ", tv";
                if (mediaString != "") mediaString = mediaString.Substring(2);
                b.Append(" media=\"" + mediaString + "\"");
            }
            // href
            if (_HypertextReference != null && _HypertextReference.Length > 0)
            {
                b.Append(" href=\"" + _HypertextReference + "\"");
            }
            // end the tag
            b.Append((doc.IsXHtml) ? " />" : ">");
            b.Append("\r\n");
        }

        #endregion

    }

}
