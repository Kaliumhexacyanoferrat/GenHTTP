using System.Runtime.CompilerServices;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public sealed class RawRequestTarget : IRawRequestTarget
{
    private ReadOnlyMemory<byte> _path = ReadOnlyMemory<byte>.Empty;

    private int _offset;

    public ReadOnlyMemory<byte>? Current { get; private set; }

    public void Apply(ReadOnlyMemory<byte> path)
    {
        _path = path;

        _offset = 0;
        Current = null;

        MoveNext();
    }

    public void Advance(int segments = 1)
    {
        while (segments-- > 0 && Current != null)
        {
            MoveNext();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MoveNext()
    {
        var span = _path.Span;
        var length = span.Length;

        if (length < 2)
        {
            Current = null;
            return;
        }

        while (_offset < length && span[_offset] == (byte)'/')
        {
            _offset++;
        }

        if (_offset >= length)
        {
            Current = null;
            return;
        }

        var start = _offset;

        var idx = span[_offset..].IndexOf((byte)'/');

        if (idx < 0)
        {
            _offset = length;
            Current = _path.Slice(start, length - start);
        }
        else
        {
            _offset += idx;
            Current = _path.Slice(start, idx);
        }
    }

}
