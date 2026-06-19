namespace GenHTTP.Api.Protocol;

/// <summary>
/// Allows cookies to be read from a header list (e.g. the "Cookie" request header).
/// </summary>
public static class CookieHeaderExtensions
{

    #region Functionality

    /// <summary>
    /// Searches the given headers for a cookie with the specified name.
    /// </summary>
    /// <param name="headers">The headers to search (typically <see cref="IRequestHeader.Headers"/>)</param>
    /// <param name="key">The name of the cookie to be looked up</param>
    /// <returns>The value of the cookie, if found</returns>
    public static ByteString? GetCookie(this IKeyValueList headers, ByteString key)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            var entry = headers[i];

            if (entry.Key != KnownHeaders.Cookie)
            {
                continue;
            }

            var found = FindCookie(entry.Value, key);

            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Searches the given headers for a cookie with the specified name.
    /// </summary>
    /// <param name="headers">The headers to search (typically <see cref="IRequestHeader.Headers"/>)</param>
    /// <param name="key">The name of the cookie to be looked up</param>
    /// <returns>The value of the cookie, if found</returns>
    public static string? GetCookie(this IKeyValueList headers, string key)
        => headers.GetCookie(new ByteString(key))?.ToString();

    /// <summary>
    /// Collects all cookies found in the given headers into a list that
    /// can be iterated or queried by name.
    /// </summary>
    /// <param name="headers">The headers to search (typically <see cref="IRequestHeader.Headers"/>)</param>
    /// <returns>The cookies found in the given headers</returns>
    public static IKeyValueList GetCookies(this IKeyValueList headers)
    {
        var cookies = new List<KeyValuePair<ByteString, ByteString>>();

        for (var i = 0; i < headers.Count; i++)
        {
            var entry = headers[i];

            if (entry.Key == KnownHeaders.Cookie)
            {
                ParseCookies(entry.Value, cookies);
            }
        }

        return new CookieList(cookies);
    }

    #endregion

    #region Parsing

    private static ByteString? FindCookie(ByteString header, ByteString key)
    {
        var memory = header.Bytes;
        var span = memory.Span;

        var keySpan = key.Bytes.Span;

        var segments = new CookieSegments(span);

        while (segments.MoveNext())
        {
            if (span[segments.Name].SequenceEqual(keySpan))
            {
                return new ByteString(memory[segments.Value]);
            }
        }

        return null;
    }

    private static void ParseCookies(ByteString header, List<KeyValuePair<ByteString, ByteString>> target)
    {
        var memory = header.Bytes;

        var segments = new CookieSegments(memory.Span);

        while (segments.MoveNext())
        {
            target.Add(new(new ByteString(memory[segments.Name]), new ByteString(memory[segments.Value])));
        }
    }

    #endregion

    #region Supporting Data Structures

    /// <summary>
    /// Walks a "Cookie" header value, yielding the name/value ranges of
    /// each "name=value" pair it finds (separated by "; ").
    /// </summary>
    private ref struct CookieSegments(ReadOnlySpan<byte> header)
    {
        private readonly ReadOnlySpan<byte> _header = header;

        private int _position = 0;

        public Range Name { get; private set; }

        public Range Value { get; private set; }

        public bool MoveNext()
        {
            while (_position < _header.Length)
            {
                while (_position < _header.Length && _header[_position] == (byte)' ')
                {
                    _position++;
                }

                if (_position >= _header.Length)
                {
                    return false;
                }

                var start = _position;

                var delimiter = _header[start..].IndexOf((byte)';');

                var segmentEnd = delimiter < 0 ? _header.Length : start + delimiter;

                var separator = _header[start..segmentEnd].IndexOf((byte)'=');

                _position = segmentEnd + 1;

                if (separator > -1)
                {
                    Name = start..(start + separator);
                    Value = (start + separator + 1)..segmentEnd;

                    return true;
                }
            }

            return false;
        }

    }

    /// <summary>
    /// A read-only list of cookies that have been parsed from one or
    /// more "Cookie" header values.
    /// </summary>
    private sealed class CookieList(List<KeyValuePair<ByteString, ByteString>> entries) : IKeyValueList
    {

        public int Count => entries.Count;

        public KeyValuePair<ByteString, ByteString> this[int index] => entries[index];

    }

    #endregion

}
