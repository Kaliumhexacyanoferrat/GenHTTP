using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Protocol;

public sealed class RequestHeaderCollection : PooledDictionary<string, string>, IHeaderCollection, IEditableHeaderCollection
{
    private const int DefaultSize = 18;

    #region Initialization

    public RequestHeaderCollection() : base(DefaultSize, StringComparer.InvariantCultureIgnoreCase)
    {

    }

    #endregion

}
