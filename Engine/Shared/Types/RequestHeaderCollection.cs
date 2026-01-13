using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RequestHeaderCollection : MultiEntryDictionary<string, string>, IHeaderCollection, IEditableHeaderCollection
{
    private const int DefaultSize = 12;

    #region Initialization

    public RequestHeaderCollection() : base(DefaultSize, StringComparer.InvariantCultureIgnoreCase)
    {

    }

    #endregion

}
