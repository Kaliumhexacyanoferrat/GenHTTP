using System.Collections;

using GenHTTP.Api.Protocol;

using Wired.IO.Http11Express.Request;

namespace GenHTTP.Adapters.WiredIO.Types;

public sealed class Query : IRequestQuery
{

    #region Get-/Setters

    public int Count => Request.QueryParameters?.Count ?? 0;

    public bool ContainsKey(string key) => Request.QueryParameters?.ContainsKey(key) ?? false;

    public bool TryGetValue(string key, out string value)
    {
        if (Request.QueryParameters?.TryGetValue(key, out var stringValue) ?? false)
        {
            value = stringValue;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string this[string key]
    {
        get
        {
            if (Request.QueryParameters?.TryGetValue(key, out var stringValue) ?? false)
            {
                return stringValue;
            }

            return string.Empty;
        }
    }

    public IEnumerable<string> Keys => Request.QueryParameters?.Keys ?? Enumerable.Empty<string>();

    public IEnumerable<string> Values
    {
        get
        {
            if (Request.QueryParameters != null)
            {
                foreach (var entry in Request.QueryParameters)
                {
                    yield return entry.Value;
                }
            }
        }
    }

    private IExpressRequest Request { get; }

    #endregion

    #region Initialization

    public Query(IExpressRequest request)
    {
        Request = request;
    }

    #endregion

    #region Functionality

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        if (Request.QueryParameters != null)
        {
            foreach (var entry in Request.QueryParameters)
            {
                yield return new(entry.Key, entry.Value);
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
