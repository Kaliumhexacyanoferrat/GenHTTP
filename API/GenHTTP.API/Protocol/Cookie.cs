using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    public class Cookie
    {

        #region Get-/Setters

        public string Name { get; }

        public string Value { get; set; }

        public DateTime? Expires { get; set; }

        public ulong? MaxAge { get; set; }

        #endregion

        #region Initialization

        public Cookie(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public Cookie(string name, string value, DateTime expires, ulong maxAge) : this(name, value)
        {
            Name = name;
            Value = value;
            Expires = expires;
            MaxAge = maxAge;
        }

        #endregion
                
    }

}
