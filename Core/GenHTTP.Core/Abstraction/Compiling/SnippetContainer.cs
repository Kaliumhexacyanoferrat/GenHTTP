using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Compiling
{

    /// <summary>
    /// Allows you to add a list of snippets as
    /// a placeholder to a document.
    /// </summary>
    public class SnippetContainer
    {
        private List<ISnippet> _Snippets;

        #region Constructors

        /// <summary>
        /// Create a new snippet container.
        /// </summary>
        public SnippetContainer()
        {
            _Snippets = new List<ISnippet>();
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// Add a snippet to this container.
        /// </summary>
        /// <param name="snippet">The snippet to add</param>
        public void Add(ISnippet snippet)
        {
            _Snippets.Add(snippet);
        }

        /// <summary>
        /// The number of snippets in this container.
        /// </summary>
        public int Count { get { return _Snippets.Count; } }

        /// <summary>
        /// Remove a snippet from this container.
        /// </summary>
        /// <param name="snippet">The snippet to remove</param>
        public void Remove(ISnippet snippet)
        {
            if (_Snippets.Contains(snippet)) _Snippets.Remove(snippet);
        }

        /// <summary>
        /// Retrieve an enumerator to iterate over this collection.
        /// </summary>
        /// <returns>The enumerator for this container</returns>
        public IEnumerator<ISnippet> GetEnumerator()
        {
            return _Snippets.GetEnumerator();
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Convert the snippets into a byte array.
        /// </summary>
        /// <returns>The content of this container</returns>
        public byte[] ToByteArray()
        {
            long length = 0;
            int pos = 0;
            foreach (ISnippet snippet in _Snippets)
            {
                length += snippet.ContentLength;
            }
            byte[] ret = new byte[length];
            foreach (ISnippet snippet in _Snippets)
            {
                byte[] toAdd = snippet.ToByteArray();
                System.Buffer.BlockCopy(toAdd, 0, ret, pos, toAdd.Length);
                pos += toAdd.Length;
            }
            return ret;
        }

        #endregion

    }

}
