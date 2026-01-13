using System.Collections;
using GenHTTP.Api.Protocol;
using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class Headers : IHeaderCollection
{

    #region Get-/Setters

    public int Count => Request?.Headers.Count ?? 0;

    public bool ContainsKey(string key) => Request?.Headers.ContainsKey(key) ?? false;

    public bool TryGetValue(string key, out string value)
    {
        if (Request?.Headers.TryGetValue(key, out var strings) ?? false)
        {
            value = strings.FirstOrDefault() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string this[string key] => Request?.Headers[key].FirstOrDefault() ?? string.Empty;

    public IEnumerable<string> Keys => Request?.Headers.Keys ?? Enumerable.Empty<string>();

    public IEnumerable<string> Values
    {
        get
        {
            if (Request != null)
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
    }

    public bool ContainsMultiple(string key)
    {
        if (Request == null) return false;

        if (!Request.Headers.TryGetValue(key, out var values))
        {
            return false;
        }

        if (values.Count > 0)
        {
            return values.Count > 1 || values.First()!.Contains(',');
        }

        return false;
    }

    private HttpRequest? Request { get; set; }

    #endregion

    #region Functionality

    internal void SetRequest(HttpRequest? request)
    {
        Request = request;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        if (Request != null)
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
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

}
