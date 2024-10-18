using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Utilities;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class RequestHeaderCollection : PooledDictionary<string, string>, IHeaderCollection, IEditableHeaderCollection
{
    private const int DefaultSize = 18;

    #region Initialization

    internal RequestHeaderCollection() : base(DefaultSize, StringComparer.InvariantCultureIgnoreCase)
    {

    }

    #endregion

}
