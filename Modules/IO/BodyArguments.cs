using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Provides access to the key/value pairs of a request body that
/// has been encoded as "application/x-www-form-urlencoded".
/// </summary>
public sealed class BodyArguments : IKeyValueList
{
    private readonly KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>[] _entries;

    #region Initialization

    /// <summary>
    /// A body that does not contain any arguments.
    /// </summary>
    public static readonly BodyArguments Empty = new([]);

    private BodyArguments(KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>[] entries)
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

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index) => _entries[index];

    #endregion

    #region Functionality

    private static BodyArguments Parse(ReadOnlyMemory<byte> memory)
    {
        if (memory.IsEmpty)
        {
            return Empty;
        }

        var entries = new List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>>();

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

    private static KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> ParsePair(ReadOnlyMemory<byte> pair)
    {
        var separator = pair.Span.IndexOf((byte)'=');

        return (separator < 0) ? new(Decode(pair), default)
                                : new(Decode(pair[..separator]), Decode(pair[(separator + 1)..]));
    }

    private static ReadOnlyMemory<byte> Decode(ReadOnlyMemory<byte> raw) => PercentEncoding.Decode(raw, decodePlus: true);

    #endregion

}
