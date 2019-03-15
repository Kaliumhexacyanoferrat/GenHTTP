using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// This class contains information about the
    /// internationalization of a <see cref="Document" /> or
    /// element.
    /// </summary>
    /// <remarks>
    /// You'll find this entity in the W3C speccs under
    /// "i18n".
    /// </remarks>
    public class Internationalization
    {
        private LanguageInfo _Language;
        private TextDirection _Direction = TextDirection.Unspecified;

        #region constructors

        /// <summary>
        /// Create a new object of this class.
        /// </summary>
        /// <param name="language">The language to use</param>
        /// <param name="direction">The text direction to use</param>
        public Internationalization(LanguageInfo language, TextDirection direction)
        {
            _Language = language;
            _Direction = direction;
        }

        /// <summary>
        /// Create a new object of this class.
        /// </summary>
        /// <param name="direction">The text direction to use</param>
        public Internationalization(TextDirection direction)
        {
            _Direction = direction;
        }

        /// <summary>
        /// Create a new object of this class.
        /// </summary>
        /// <param name="language">The language to use</param>
        public Internationalization(LanguageInfo language)
        {
            _Language = language;
        }

        /// <summary>
        /// Create a new object of this class.
        /// </summary>
        public Internationalization()
        {

        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The language to use.
        /// </summary>
        public LanguageInfo Language
        {
            get { return _Language; }
            set { _Language = value; }
        }

        /// <summary>
        /// The text direction to use.
        /// </summary>
        public TextDirection TextDirection
        {
            get { return _Direction; }
            set { _Direction = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize the internationalization information.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="isXHtml">Defines, whether XHTML syntax should be used</param>
        internal void Serialize(StringBuilder b, bool isXHtml)
        {
            b.Append(Serialize(isXHtml));
        }

        /// <summary>
        /// Serialize this object to (X)HTML.
        /// </summary>
        /// <param name="isXHtml">Specify, whether XHTML syntax should be used</param>
        /// <returns>The serialized object</returns>
        internal string Serialize(bool isXHtml)
        {
            string ret = "";
            if (_Language != null)
            {
                ret += " lang=\"" + Language.LanguageString + "\"";
                if (isXHtml) ret += " xml:lang=\"" + Language.LanguageString + "\"";
            }
            if (_Direction != TextDirection.Unspecified)
            {
                ret += " dir=\"" + ((TextDirection == TextDirection.LeftToRight) ? "ltr" : "rtl") + "\"";
            }
            return ret;
        }

        /// <summary>
        /// Serialize this internationalization object to a XML node.
        /// </summary>
        /// <param name="node">The node to serialize to</param>
        public void ToXml(Setting node)
        {
            if (_Direction != TextDirection.Unspecified) node.Attributes["direction"] = _Direction.ToString();
            if (_Language != null) node.Attributes["language"] = _Language.LanguageString;
        }

        /// <summary>
        /// Retrieve an internationalization object from a XML element node.
        /// </summary>
        /// <param name="node">The node to read</param>
        /// <returns>The matching internationalization object</returns>
        public static Internationalization FromXml(Setting node)
        {
            Internationalization i = new Internationalization();
            switch (node.Attributes["direction"])
            {
                case "RightToLeft": i.TextDirection = TextDirection.RightToLeft; break;
                case "LeftToRight": i.TextDirection = TextDirection.LeftToRight; break;
                default: i.TextDirection = TextDirection.Unspecified; break;
            }
            if (node.Attributes["language"] != null) i.Language = new LanguageInfo(node.Attributes["language"]);
            return i;
        }

        #endregion

    }

}
