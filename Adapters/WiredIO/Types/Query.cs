using System.Collections;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Adapters.WiredIO.Types;

public sealed class Query : IRequestQuery
{

    #region Get-/Setters

    public int Count => Request.Query.Count;

    public bool ContainsKey(string key) => Request.Query.ContainsKey(key);

    public bool TryGetValue(string key, out string value)
    {
        if (Request.Query.TryGetValue(key, out var stringValue))
        {
            value = stringValue.FirstOrDefault() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string this[string key] => Request.Query[key][0] ?? string.Empty;

    public IEnumerable<string> Keys => Request.Query.Keys;

    public IEnumerable<string> Values
    {
        get
        {
            foreach (var entry in Request.Query)
            {
                foreach (var value in entry.Value)
                {
                    if (value != null) yield return value;
                }
            }
        }
    }

    private HttpRequest Request { get; }

    #endregion

    #region Initialization

    public Query(HttpRequest request)
    {
        Request = request;
    }

    #endregion

    #region Functionality

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var entry in Request.Query)
        {
            foreach (var stringEntry in entry.Value)
            {
                if (stringEntry != null)
                {
                    yield return new(entry.Key, stringEntry);
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Lifecycle

    public void Dispose()
    {

    }

    #endregion

}
