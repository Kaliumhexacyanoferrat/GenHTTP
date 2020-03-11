using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Core.Utilities;

namespace GenHTTP.Core.Protocol
{
    
    internal class RequestQuery : PooledDictionary<string, string>, IRequestQuery
    {
        private const int DEFAULT_SIZE = 12;

        internal RequestQuery() : base(DEFAULT_SIZE, StringComparer.OrdinalIgnoreCase)
        {

        }

    }

}
