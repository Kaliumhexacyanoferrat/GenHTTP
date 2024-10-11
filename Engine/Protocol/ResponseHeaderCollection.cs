using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol;

internal sealed class ResponseHeaderCollection : PooledDictionary<string, string>, IHeaderCollection, IEditableHeaderCollection
{
    private const int DefaultSize = 18;

    private static readonly HashSet<string> ReservedHeaders = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Date", "Connection", "Content-Type", "Content-Encoding", "Content-Length",
        "Transfer-Encoding", "Last-Modified", "Expires"
    };

    #region Initialization

    internal ResponseHeaderCollection() : base(DefaultSize, StringComparer.InvariantCultureIgnoreCase)
    {

    }

    #endregion

    #region Get-/Setters

    public override string this[string key]
    {
        get => base[key];
        set
        {
            CheckKey(key);
            base[key] = value;
        }
    }

    #endregion

    #region Functionality

    public override void Add(string key, string value)
    {
        CheckKey(key);
        base.Add(key, value);
    }

    public override void Add(KeyValuePair<string, string> item)
    {
        CheckKey(item.Key);
        base.Add(item);
    }

    private static void CheckKey(string key)
    {
        if (ReservedHeaders.Contains(key))
        {
            throw new ArgumentException($"Header '{key}' cannot be set via header. Please use the designated property instead.");
        }
    }

    #endregion

}
