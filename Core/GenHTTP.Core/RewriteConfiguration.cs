using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP
{

    /// <summary>
    /// Stores configurations of rewrites.
    /// </summary>
    public class RewriteConfiguration
    {
        private string _URL;
        private string _To;
        private bool _Regex;

        #region Constructors

        /// <summary>
        /// Create a new rewrite entry.
        /// </summary>
        /// <param name="url">The URL to rewrite</param>
        /// <param name="to">The destination address</param>
        /// <param name="regex">Specifies, whether regular expressions should be used</param>
        /// <remarks>
        /// You can use regular expressions in the URL field. If you have enabled regular
        /// expressions, you can use a placeholder for every match group in the to field ($1 ... $n).
        /// </remarks>
        public RewriteConfiguration(string url, string to, bool regex)
        {
            _URL = url;
            _To = to;
            _Regex = regex;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The URL to rewrite.
        /// </summary>
        public string Url
        {
            get { return _URL; }
        }

        /// <summary>
        /// The destination address.
        /// </summary>
        public string To
        {
            get { return _To; }
        }

        /// <summary>
        /// Specifies, whether to use regular expressions or not.
        /// </summary>
        public bool Regex
        {
            get { return _Regex; }
        }

        #endregion

    }

}
