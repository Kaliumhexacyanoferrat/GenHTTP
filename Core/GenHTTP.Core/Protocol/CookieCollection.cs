using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class CookieCollection : Dictionary<string, Cookie>, ICookieCollection
    {

        internal CookieCollection() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

    }

}
