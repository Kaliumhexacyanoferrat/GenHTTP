using System.Collections;
using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class ResponseHeaderCollection : IHeaderCollection, IEditableHeaderCollection
{
    private const int DefaultSize = 12;

    private readonly Dictionary<string, string> _source = new(DefaultSize, StringComparer.InvariantCultureIgnoreCase);

    private static readonly HashSet<string> ReservedHeaders = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Date",
        "Connection",
        "Content-Type",
        "Content-Encoding",
        "Content-Length",
        "Transfer-Encoding",
        "Last-Modified",
        "Expires"
    };

    #region Get-/Setters

    public bool IsReadOnly => false;

    int ICollection<KeyValuePair<string, string>>.Count => _source.Count;

    int IReadOnlyCollection<KeyValuePair<string, string>>.Count => _source.Count;

    public string this[string key]
    {
        get => _source[key];
        set
        {
            CheckKey(key);
            _source[key] = value;
        }
    }

    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => _source.Keys;

    ICollection<string> IDictionary<string, string>.Values => _source.Values;

    ICollection<string> IDictionary<string, string>.Keys => _source.Keys;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => _source.Values;

    #endregion

    #region Functionality

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<string, string> item)
    {
        CheckKey(item.Key);
        _source.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _source.Clear();
    }

    public bool Contains(KeyValuePair<string, string> item) => _source.Contains(item);

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        throw new NotSupportedException();
    }

    public bool Remove(KeyValuePair<string, string> item) => _source.Remove(item.Key);

    public void Add(string key, string value)
    {
        CheckKey(key);
        _source.Add(key, value);
    }

    bool IDictionary<string, string>.ContainsKey(string key) => _source.ContainsKey(key);

    public bool Remove(string key) => _source.Remove(key);

    bool IDictionary<string, string>.TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _source.TryGetValue(key, out value);

    bool IReadOnlyDictionary<string, string>.ContainsKey(string key) => _source.ContainsKey(key);

    bool IReadOnlyDictionary<string, string>.TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _source.TryGetValue(key, out value);

    private static void CheckKey(string key)
    {
        if (ReservedHeaders.Contains(key))
        {
            throw new ArgumentException($"Header '{key}' cannot be set via header. Please use the designated property instead.");
        }
    }

    #endregion

}
