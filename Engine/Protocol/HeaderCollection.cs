using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol
{

    internal class HeaderCollection : PooledDictionary<string, string>, IHeaderCollection, IEditableHeaderCollection
    {
        private const int DEFAULT_SIZE = 18;

        internal HeaderCollection() : base(DEFAULT_SIZE, StringComparer.InvariantCultureIgnoreCase)
        {

        }

    }

}
