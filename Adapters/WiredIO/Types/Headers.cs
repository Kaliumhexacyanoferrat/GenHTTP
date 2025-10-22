using System.Collections;

using GenHTTP.Api.Protocol;

using Wired.IO.Http11Express.Request;

namespace GenHTTP.Adapters.WiredIO.Types;

public sealed class Headers : IHeaderCollection
{

    #region Get-/Setters

    public int Count => Request.Headers.Count;

    public bool ContainsKey(string key) => Request.Headers.ContainsKey(key);

    public bool TryGetValue(string key, out string value)
    {
        if (Request.Headers.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string this[string key] => ContainsKey(key) ? Request.Headers[key] : string.Empty;

    public IEnumerable<string> Keys => Request.Headers.Keys;

    public IEnumerable<string> Values
    {
        get
        {
            foreach (var entry in Request.Headers)
            {
                yield return entry.Value;
            }
        }
    }

    private IExpressRequest Request { get; }

    #endregion

    #region Initialization

    public Headers(IExpressRequest request)
    {
        Request = request;
    }

    #endregion

    #region Functionality

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var entry in Request.Headers)
        {
            yield return new(entry.Key, entry.Value);
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
