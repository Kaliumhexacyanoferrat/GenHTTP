using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP
{

    /// <summary>
    /// Represents a HTTP cookie.
    /// </summary>
    [Serializable]
    public class HttpCookie
    {
        private string _Name;
        private string _Value;
        private DateTime? _Expires;
        private ulong? _MaxAge;

        /// <summary>
        /// Create a new HTTP cookie.
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The content of the cookie</param>
        public HttpCookie(string name, string value)
        {
            _Name = name;
            _Value = value;
        }

        /// <summary>
        /// Create a new HTTP cookie.
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The content of the cookie</param>
        /// <param name="expires">The expire date of the cookie</param>
        /// <param name="maxAge">The maximum age of the cookie</param>
        public HttpCookie(string name, string value, DateTime expires, ulong maxAge) : this(name, value)
        {
            _Name = name;
            _Value = value;
            _Expires = expires;
            _MaxAge = maxAge;
        }

        #region get-/setters

        /// <summary>
        /// The name of the cookie.
        /// </summary>
        public string Name
        {
            get { return _Name; }
        }

        /// <summary>
        /// The content of the cookie.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// The expire date of the cookie.
        /// </summary>
        public DateTime? Expires
        {
            get { return _Expires; }
            set { _Expires = value; }
        }

        /// <summary>
        /// The maximum age of the cookie.
        /// </summary>
        public ulong? MaxAge
        {
            get { return _MaxAge; }
            set { _MaxAge = value; }
        }

        #endregion
        
    }

}
