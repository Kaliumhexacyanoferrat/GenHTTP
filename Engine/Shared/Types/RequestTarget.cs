using System.Runtime.CompilerServices;
using System.Text;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RequestTarget : IRequestTarget
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

    public ReadOnlyMemory<byte>? Next(int offset)
    {
        if (Current == null)
        {
            return null;
        }

        if (offset == 0)
        {
            return Current.Value.Value;
        }

        var span = _path.Span;
        var length = span.Length;
        var i = _offset;

        for (var skip = 1; skip <= offset; skip++)
        {
            while (i < length && span[i] == (byte)'/')
            {
                i++;
            }

            if (i >= length)
            {
                return null;
            }

            var start = i;
            var idx = span[i..].IndexOf((byte)'/');

            if (skip == offset)
            {
                return idx < 0 ? _path.Slice(start, length - start) : _path.Slice(start, idx);
            }

            i += idx < 0 ? (length - start) : idx;
        }

        return null;
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
