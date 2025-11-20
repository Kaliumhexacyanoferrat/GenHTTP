using System.Collections;
using GenHTTP.Api.Protocol;
using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class Query : IRequestQuery
{

    #region Get-/Setters

    public int Count => Request?.Query.Count ?? 0;

    public bool ContainsKey(string key) => Request?.Query.ContainsKey(key) ?? false;

    public bool TryGetValue(string key, out string value)
    {
        if (Request?.Query.TryGetValue(key, out var stringValue) ?? false)
        {
            value = stringValue.FirstOrDefault() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string this[string key] => Request?.Query[key][0] ?? string.Empty;

    public IEnumerable<string> Keys => Request?.Query.Keys ?? Enumerable.Empty<string>();

    public IEnumerable<string> Values
    {
        get
        {
            if (Request != null)
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
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

}
