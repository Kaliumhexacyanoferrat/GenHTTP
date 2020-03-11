﻿using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Core.Utilities;

namespace GenHTTP.Core.Protocol
{

    internal class HeaderCollection : PooledDictionary<string, string>, IHeaderCollection
    {
        private const int DEFAULT_SIZE = 16;

        internal HeaderCollection() : base(DEFAULT_SIZE, StringComparer.InvariantCultureIgnoreCase)
        {

        }

    }

}
