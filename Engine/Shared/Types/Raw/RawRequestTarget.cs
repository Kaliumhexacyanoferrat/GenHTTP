using System.Runtime.CompilerServices;
using System.Text;

using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public sealed class RawRequestTarget : IRawRequestTarget
{
    private ReadOnlyMemory<byte> _path = ReadOnlyMemory<byte>.Empty;

    private int _offset;
    private int _segmentStart;

    public PathSegment? Current { get; private set; }

    public bool IsLast
    {
        get
        {
            if (Current == null)
            {
                return false;
            }

            var span = _path.Span;
            var length = span.Length;
            var i = _offset;

            while (i < length && span[i] == (byte)'/')
                i++;

            return i >= length;
        }
    }

    public bool HasTrailingSlash
    {
        get
        {
            var span = _path.Span;
            return span.Length > 0 && span[^1] == (byte)'/';
        }
    }

    public void Apply(ReadOnlyMemory<byte> path)
    {
        _path = path;

        _offset = 0;
        _segmentStart = 0;
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
        _segmentStart = start;

        var idx = span[_offset..].IndexOf((byte)'/');

        if (idx < 0)
        {
            _offset = length;
            Current = new(_path.Slice(start, length - start));
        }
        else
        {
            _offset += idx;
            Current = new(_path.Slice(start, idx));
        }
    }

    public string AsString(bool decode = true, bool remainingOnly = false)
    {
        ReadOnlyMemory<byte> slice;

        if (remainingOnly)
        {
            if (Current == null)
            {
                return string.Empty;
            }

            slice = _path[_segmentStart..];
        }
        else
        {
            slice = _path;
        }

        var stringPath = Encoding.ASCII.GetString(slice.Span);

        if (decode && stringPath.Contains('%'))
        {
            return Uri.UnescapeDataString(stringPath);
        }

        return stringPath;
    }

}
