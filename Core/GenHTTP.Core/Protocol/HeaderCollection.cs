using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class HeaderCollection : Dictionary<string, string>, IHeaderCollection
    {

        internal HeaderCollection() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

    }

}
