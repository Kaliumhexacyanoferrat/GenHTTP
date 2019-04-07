using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Represents a cookie that can be send to or received from a client.
    /// </summary>
    public class Cookie
    {

        #region Get-/Setters

        /// <summary>
        /// The name of the cookie.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value of the cookie.
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// The number of seconds after the cookie will be discarded by the client.
        /// </summary>
        public ulong? MaxAge { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new cookie with the given name and value.
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The value of the cookie</param>
        public Cookie(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Creates a new cookie with the given name and value.
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The value of the cookie</param>
        /// <param name="maxAge">The number of seconds until the cookie will be discarded</param>
        public Cookie(string name, string value, ulong maxAge) : this(name, value)
        {
            Name = name;
            Value = value;
            MaxAge = maxAge;
        }

        #endregion
                
    }

}
