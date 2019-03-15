using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// This class saves additional meta data for a <see cref="Document"/>.
    /// </summary>
    public class DocumentMetaInformationCollection
    {
        private List<DocumentMetaInformation> _MetaInformation;

        #region constructors

        /// <summary>
        /// Create a new meta data collection.
        /// </summary>
        internal DocumentMetaInformationCollection()
        {
            _MetaInformation = new List<DocumentMetaInformation>();
        }

        #endregion

        #region collection functionality

        /// <summary>
        /// Check, whether the collection does already contain
        /// a given meta information.
        /// </summary>
        /// <param name="info">The meta information to check</param>
        /// <returns>true, if the document contains meta information like the given</returns>
        public bool Contains(DocumentMetaInformation info)
        {
            foreach (DocumentMetaInformation i in _MetaInformation)
            {
                if (i.Name == info.Name)
                {
                    if (i.Type == info.Type)
                    {
                        if (i.Internationalization.Language != null && info.Internationalization.Language != null)
                        {
                            return i.Internationalization.Language.LanguageString == info.Internationalization.Language.LanguageString;
                        }
                        else
                        {
                            return i.Internationalization.Language == info.Internationalization.Language;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check, whether the collection does already contain
        /// a meta tag with this name.
        /// </summary>
        /// <param name="metaName">The name to check</param>
        /// <returns>true, if the document contains a meta tag with this name</returns>
        public bool Contains(string metaName)
        {
            return _MetaInformation.Where((DocumentMetaInformation i) => (i.Name == metaName)).Count() > 0;
        }

        /// <summary>
        /// Insert new meta information.
        /// </summary>
        /// <param name="info">The meta information to insert</param>
        /// <remarks>
        /// This will overwrite already existing meta information
        /// with this attributes.
        /// </remarks>
        public void Add(DocumentMetaInformation info)
        {
            if (Contains(info))
            {
                DocumentMetaInformation toUpdate = null;
                foreach (DocumentMetaInformation i in _MetaInformation)
                {
                    if (i.Name == info.Name)
                    {
                        if (i.Type == info.Type)
                        {
                            if (i.Internationalization.Language != null && info.Internationalization.Language != null)
                            {
                                if (i.Internationalization.Language.LanguageString == info.Internationalization.Language.LanguageString)
                                {
                                    toUpdate = i;
                                    break;
                                }
                            }
                            else
                            {
                                toUpdate = i;
                                break;
                            }
                        }
                    }
                }
                toUpdate.Content = info.Content;
                toUpdate.Scheme = info.Scheme;
                toUpdate.Internationalization.TextDirection = info.Internationalization.TextDirection;
            }
            else
            {
                _MetaInformation.Add(info);
            }
        }

        /// <summary>
        /// Insert new meta information.
        /// </summary>
        /// <param name="name">The name of the meta tag</param>
        /// <param name="content">The content of the meta tag</param>
        /// <remarks>The newly created <see cref="DocumentMetaInformation"/> object</remarks>
        public DocumentMetaInformation Add(string name, string content)
        {
            DocumentMetaInformation info = new DocumentMetaInformation(name, content);
            Add(info);
            return info;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize the meta information to a string builder.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="doc">The document this collection relates to</param>
        internal void Serialize(StringBuilder b, Document doc)
        {
            foreach (DocumentMetaInformation meta in _MetaInformation)
            {
                meta.Serialize(b, doc);
            }
        }

        #endregion

    }

}
