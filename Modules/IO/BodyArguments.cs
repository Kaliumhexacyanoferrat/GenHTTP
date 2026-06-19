using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Provides access to the key/value pairs of a request body that
/// has been encoded as "application/x-www-form-urlencoded".
/// </summary>
public sealed class BodyArguments : IKeyValueList
{
    private readonly KeyValuePair<ByteString, ByteString>[] _entries;

    #region Initialization

    /// <summary>
    /// A body that does not contain any arguments.
    /// </summary>
    public static readonly BodyArguments Empty = new([]);

    private BodyArguments(KeyValuePair<ByteString, ByteString>[] entries)
    {
        _entries = entries;
    }

    /// <summary>
    /// Reads and parses the given body as form encoded content.
    /// </summary>
    /// <param name="body">The body to be parsed</param>
    /// <returns>The arguments found within the body</returns>
    public static async ValueTask<BodyArguments> CreateAsync(IRequestBody body)
    {
        var memory = await body.AsMemoryAsync();

        return Parse(memory);
    }

    #endregion

    #region Get-/Setters

    public int Count => _entries.Length;

    public KeyValuePair<ByteString, ByteString> this[int index] => _entries[index];

    #endregion

    #region Functionality

    private static BodyArguments Parse(ReadOnlyMemory<byte> memory)
    {
        if (memory.IsEmpty)
        {
            return Empty;
        }

        var entries = new List<KeyValuePair<ByteString, ByteString>>();

        var remaining = memory;

        while (!remaining.IsEmpty)
        {
            var separator = remaining.Span.IndexOf((byte)'&');

            var pair = (separator < 0) ? remaining : remaining[..separator];

            if (!pair.IsEmpty)
            {
                entries.Add(ParsePair(pair));
            }

            remaining = (separator < 0) ? default : remaining[(separator + 1)..];
        }

        return new BodyArguments(entries.ToArray());
    }

    private static KeyValuePair<ByteString, ByteString> ParsePair(ReadOnlyMemory<byte> pair)
    {
        var separator = pair.Span.IndexOf((byte)'=');

        return (separator < 0) ? new(Decode(pair), default)
                                : new(Decode(pair[..separator]), Decode(pair[(separator + 1)..]));
    }

    /// <summary>
    /// Decodes a percent- and "+"-encoded token, only allocating a new
    /// buffer if the token actually requires decoding.
    /// </summary>
    private static ByteString Decode(ReadOnlyMemory<byte> raw)
    {
        var source = raw.Span;

        if (source.IndexOfAny((byte)'%', (byte)'+') < 0)
        {
            return new ByteString(raw);
        }

        var target = new byte[source.Length];

        var written = PercentEncoding.Decode(source, target, decodePlus: true);

        return new ByteString(target.AsMemory(0, written));
    }

    #endregion

}
