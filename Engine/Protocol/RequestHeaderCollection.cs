using System;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol
{

    internal sealed class RequestHeaderCollection : PooledDictionary<string, string>, IHeaderCollection, IEditableHeaderCollection
    {
        private const int DEFAULT_SIZE = 18;

        #region Initialization

        internal RequestHeaderCollection() : base(DEFAULT_SIZE, StringComparer.InvariantCultureIgnoreCase)
        {

        }

        #endregion

    }

}
