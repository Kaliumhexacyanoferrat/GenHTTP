using System.Collections;
using GenHTTP.Api.Protocol;
using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class Headers : IHeaderCollection
{

    #region Get-/Setters

    public int Count => Request.Headers.Count;

    public bool ContainsKey(string key) => Request.Headers.ContainsKey(key);

    public bool TryGetValue(string key, out string value)
    {
        if (Request.Headers.TryGetValue(key, out var strings))
        {
            value = strings.FirstOrDefault() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string this[string key] => Request.Headers[key].FirstOrDefault() ?? string.Empty;

    public IEnumerable<string> Keys => Request.Headers.Keys;

    public IEnumerable<string> Values
    {
        get
        {
            foreach (var entry in Request.Headers)
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

    public Headers(HttpRequest request)
    {
        Request = request;
    }

    #endregion

    #region Functionality

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var entry in Request.Headers)
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
