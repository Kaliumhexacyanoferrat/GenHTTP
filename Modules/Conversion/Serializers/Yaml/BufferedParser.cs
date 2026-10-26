using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

/// <summary>
/// Reads all events of a YAML document upfront to reject deeply nested
/// documents before they are deserialized.
/// </summary>
/// <remarks>
/// YamlDotNet wraps and re-throws errors on every nesting level, which consumes
/// stack space proportional to the depth of the document. Rejecting such documents
/// while scanning keeps the stack flat, as the parser itself is not recursive.
/// </remarks>
internal sealed class BufferedParser : IParser
{
    private readonly List<ParsingEvent> _events;

    private int _index = -1;

    #region Get-/Setters

    public ParsingEvent? Current => (_index >= 0 && _index < _events.Count) ? _events[_index] : null;

    #endregion

    #region Initialization

    private BufferedParser(List<ParsingEvent> events)
    {
        _events = events;
    }

    internal static BufferedParser Read(IParser source, int maximumDepth)
    {
        var events = new List<ParsingEvent>();

        var depth = 0;

        while (source.MoveNext())
        {
            var current = source.Current;

            if (current == null)
            {
                break;
            }

            if (current is MappingStart or SequenceStart)
            {
                if (++depth > maximumDepth)
                {
                    throw new YamlException(current.Start, current.End, $"Document exceeds the maximum nesting depth of {maximumDepth}");
                }
            }
            else if (current is MappingEnd or SequenceEnd)
            {
                depth--;
            }

            events.Add(current);
        }

        return new BufferedParser(events);
    }

    #endregion

    #region Functionality

    public bool MoveNext()
    {
        if (_index < _events.Count)
        {
            _index++;
        }

        return _index < _events.Count;
    }

    #endregion

}
