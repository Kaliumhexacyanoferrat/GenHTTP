namespace GenHTTP.Api.Protocol;

/// <summary>
/// An iterable list of key/value pairs read from
/// the request buffer.
/// </summary>
/// <remarks>
/// Does not implement IEnumerable to stay allocation-free. Does
/// not use dictionary semantics as we can have multiple headers
/// with the same name.
/// </remarks>
public interface IKeyValueList
{

    /// <summary>
    /// The number of entries in this list.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Fetches the entry with the given index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fetch</param>
    KeyValuePair<ByteString, ByteString> this[int index] { get; }

}
