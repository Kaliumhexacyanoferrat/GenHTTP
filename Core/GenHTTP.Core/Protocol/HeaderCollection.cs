using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class HeaderCollection : Dictionary<string, string>, IHeaderCollection
    {
        private const int DEFAULT_SIZE = 32;

        internal HeaderCollection() : base(DEFAULT_SIZE, StringComparer.InvariantCultureIgnoreCase)
        {

        }

    }

}
