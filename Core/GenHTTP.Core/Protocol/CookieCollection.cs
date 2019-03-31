using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Core.Protocol
{

    internal class CookieCollection : Dictionary<string, Cookie>, ICookieCollection
    {

        internal CookieCollection() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

    }

}
