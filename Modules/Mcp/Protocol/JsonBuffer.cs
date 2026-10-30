using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GenHTTP.Modules.Mcp.Protocol;

/// <summary>
/// A pooled buffer with a JSON writer, cached per thread so that writing a
/// response does not allocate apart from the final result.
/// </summary>
/// <remarks>
/// Must not be held across an await.
/// </remarks>
internal sealed class JsonBuffer : IBufferWriter<byte>, IDisposable
{
    private const int InitialSize = 4096;

    private const int RetainedSize = 1024 * 1024;

    // answers are JSON for a machine to read, never embedded in HTML
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        SkipValidation = true
    };

    [ThreadStatic]
    private static JsonBuffer? _cached;

    private byte[] _buffer;

    private int _written;

    #region Get-/Setters

    internal Utf8JsonWriter Writer { get; }

    internal ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);

    #endregion

    #region Initialization

    private JsonBuffer()
    {
        _buffer = ArrayPool<byte>.Shared.Rent(InitialSize);
        Writer = new Utf8JsonWriter(this, WriterOptions);
    }

    internal static JsonBuffer Rent()
    {
        var buffer = _cached;

        if (buffer != null)
        {
            _cached = null;
            return buffer;
        }

        return new JsonBuffer();
    }

    #endregion

    #region Functionality

    internal ReadOnlySpan<byte> Flush()
    {
        Writer.Flush();
        return WrittenSpan;
    }

    internal byte[] ToArray() => Flush().ToArray();

    public void Advance(int count) => _written += count;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer.AsMemory(_written);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer.AsSpan(_written);
    }

    private void Ensure(int sizeHint)
    {
        var required = _written + Math.Max(sizeHint, 1);

        if (required <= _buffer.Length)
        {
            return;
        }

        var grown = ArrayPool<byte>.Shared.Rent(Math.Max(required, _buffer.Length * 2));

        WrittenSpan.CopyTo(grown);

        ArrayPool<byte>.Shared.Return(_buffer);

        _buffer = grown;
    }

    public void Dispose()
    {
        Writer.Reset(this);

        _written = 0;

        if (_cached == null)
        {
            if (_buffer.Length > RetainedSize)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = ArrayPool<byte>.Shared.Rent(InitialSize);
            }

            _cached = this;
        }
        else
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = [];
        }
    }

    #endregion

}
