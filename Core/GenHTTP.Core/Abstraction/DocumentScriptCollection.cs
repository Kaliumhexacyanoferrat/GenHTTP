using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// Stores scripts for a <see cref="Document" />.
    /// </summary>
    public class DocumentScriptCollection
    {
        private HashSet<DocumentScript> _Scripts;

        #region Constructors

        /// <summary>
        /// Create a new script collection.
        /// </summary>
        internal DocumentScriptCollection()
        {
            _Scripts = new HashSet<DocumentScript>();
        }

        #endregion

        #region Collection handling

        /// <summary>
        /// Add a new script to the document.
        /// </summary>
        /// <param name="script">The script to add</param>
        public void Add(DocumentScript script)
        {
            if (script == null) throw new ArgumentNullException();
            _Scripts.Add(script);
        }

        /// <summary>
        /// Add a new, external JavaScript.
        /// </summary>
        /// <param name="source">The URL of the source file</param>
        public void Add(string source)
        {
            if (source == null || source.Length == 0) throw new ArgumentException();
            Add(new DocumentScript(source));
        }

        /// <summary>
        /// Remove a script from the collection.
        /// </summary>
        /// <param name="script">The script to remove</param>
        public void Remove(DocumentScript script)
        {
            if (script == null) throw new ArgumentNullException();
            if (_Scripts.Contains(script)) _Scripts.Remove(script);
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this collection to a document.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="doc">The related document</param>
        internal void Serialize(StringBuilder b, Document doc)
        {
            string offset = "  ";
            foreach (DocumentScript script in _Scripts)
            {
                script.Serialize(b, doc, offset);
            }
        }

        #endregion

    }

}
