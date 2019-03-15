using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP
{

    /// <summary>
    /// Stores the transmitted cookies for a <see cref="HttpRequest"/>.
    /// </summary>
    [Serializable]
    public class CookieCollection
    {
        private Dictionary<string, HttpCookie> _Cookies;

        internal CookieCollection()
        {
            _Cookies = new Dictionary<string, HttpCookie>();
        }

        #region get-/setters

        /// <summary>
        /// Retrieve a cookie.
        /// </summary>
        /// <param name="cookie">The name of the cookie</param>
        /// <returns>The requested cookie</returns>
        public HttpCookie this[string cookie]
        {
            get
            {
                if (_Cookies.ContainsKey(cookie)) return _Cookies[cookie];
                return null;
            }
        }

        /// <summary>
        /// Retrieve the number of cookies in this collection.
        /// </summary>
        public int Count
        {
            get { return _Cookies.Count; }
        }

        #endregion

        internal void Add(string name, string value)
        {
            if (_Cookies.ContainsKey(name))
            {
                _Cookies[name].Value = value;
            }
            else
            {
                _Cookies.Add(name, new HttpCookie(name, value));
            }
        }

        /// <summary>
        /// Retrieve the enumerator for this collection.
        /// </summary>
        /// <returns>The enumerator to iterate over all cookies</returns>
        public IEnumerator<HttpCookie> GetEnumerator()
        {
            return _Cookies.Values.GetEnumerator();
        }

        /// <summary>
        /// Check, whether a cookie exists or not.
        /// </summary>
        /// <param name="name">The name of the cookie to check for existance</param>
        /// <returns>true, if the cookie exists</returns>
        public bool Exists(string name)
        {
            return _Cookies.ContainsKey(name);
        }

    }

}
