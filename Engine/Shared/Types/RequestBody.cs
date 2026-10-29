using System.IO.Pipelines;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Engine.Shared.Types;

/// <summary>
/// Owns the lifecycle of the body of a request, from the point the
/// header has been parsed until the connection is ready for the
/// next request.
/// </summary>
public sealed class RequestBody : IRequestBody
{
    private static readonly ReadOnlyMemory<byte> ChunkedValue = "chunked"u8.ToArray();

    private readonly RequestHeader _header;

    private readonly ContentLengthFraming _contentLength = new();

    private readonly ChunkedFraming _chunked = new();

    private PipeReader? _reader;

    private SequencePosition _start;

    private BodyState _state;

    private bool _present;

    private IBodyFraming? _framing;

    private Stream? _stream;

    private ReadOnlyMemory<byte>? _memory;

    private Func<IRequestBody, IRequestBody>? _wrapper;

    private IRequestBody? _wrapped;

    #region Get-/Setters

    /// <summary>
    /// The length of the body as declared by the client, if known.
    /// </summary>
    public ulong? Length { get; private set; }

    internal bool Present => _present;

    internal bool Opened => _state != BodyState.Pending;

    private PipeReader Reader => _reader ?? throw new InvalidOperationException("Reader has not been initialized");

    private IBodyFraming Framing => _framing ?? throw new InvalidOperationException("Request body has not been opened");

    #endregion

    #region Initialization

    internal RequestBody(RequestHeader header)
    {
        _header = header;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Apply(PipeReader reader, SequencePosition start)
    {
        _reader = reader;
        _start = start;
        _state = BodyState.Pending;

        _wrapper = null;
        _wrapped = null;

        var headers = _header.Headers;

        _present = headers.ContainsKey(KnownHeaders.ContentLength) || headers.ContainsKey(KnownHeaders.TransferEncoding);
    }

    internal void Apply() => Clear();

    internal void Reset()
    {
        if (_wrapped is IDisposable disposableWrapped)
        {
            disposableWrapped.Dispose();
        }

        Clear();
    }

    private void Clear()
    {
        _reader = null;
        _start = default;
        _state = BodyState.Pending;
        _present = false;

        if (_framing != null)
        {
            _framing.Reset();
            _framing = null;
        }

        Length = null;

        _stream = null;
        _memory = null;

        _wrapper = null;
        _wrapped = null;
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Moves the reader past the header and provides access to the body, if any.
    /// </summary>
    internal IRequestBody? Open()
    {
        if (_present)
        {
            ResolveFraming();
        }

        Release();

        if (!_present)
        {
            return null;
        }

        if (_wrapper != null)
        {
            return _wrapped = _wrapper(this);
        }

        return this;
    }

    internal void Wrap(Func<IRequestBody, IRequestBody> wrapper) => _wrapper = wrapper;

    /// <summary>
    /// Hands the connection over to another protocol.
    /// </summary>
    internal PipeReader Upgrade()
    {
        var reader = Reader;

        Release();

        _state = BodyState.Upgraded;
        _present = false;

        return reader;
    }

    /// <summary>
    /// Consumes whatever is left of the body, so that the connection
    /// is positioned at the start of the next request.
    /// </summary>
    /// <returns>true, if the connection can be used for another request</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<bool> FinishAsync()
    {
        if (_state == BodyState.Upgraded)
        {
            return new(false);
        }

        if (!_present)
        {
            Release();
            return new(true);
        }

        var framing = _framing ?? TryResolveUnopened();

        return (framing != null) ? framing.SkipAsync() : new(false);
    }

    /// <summary>
    /// Resolves the framing of a body nobody has opened, so it can be skipped.
    /// </summary>
    /// <returns>null, if the framing is invalid and the body cannot be skipped</returns>
    private IBodyFraming? TryResolveUnopened()
    {
        IBodyFraming framing;

        try
        {
            framing = ResolveFraming();
        }
        catch (ProviderException)
        {
            return null;
        }

        Release();

        return framing;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Release()
    {
        if (_state == BodyState.Pending)
        {
            _reader?.AdvanceTo(_start);
            _state = BodyState.Opened;
        }
    }

    private IBodyFraming ResolveFraming()
    {
        var headers = _header.Headers;

        var contentLength = headers.GetEntry(KnownHeaders.ContentLength);

        if (contentLength != null)
        {
            if (!ulong.TryParse(contentLength.Value.Bytes.Span, out var length) || length > long.MaxValue)
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Content-Length header has an invalid value");
            }

            Length = length;

            _contentLength.Apply(Reader, (long)length);

            return _framing = _contentLength;
        }

        var transferEncoding = headers.GetEntry(KnownHeaders.TransferEncoding);

        if (transferEncoding != null && transferEncoding.Value.Bytes.Span.SequenceEqual(ChunkedValue.Span))
        {
            _chunked.Apply(Reader);

            return _framing = _chunked;
        }

        throw new ProviderException(ResponseStatus.NotImplemented, "Request body has neither Content-Length nor Transfer-Encoding: chunked");
    }

    #endregion

    #region Functionality

    public Stream AsStream()
    {
        if (_stream != null)
        {
            return _stream;
        }

        return _stream = new BodyStream(Framing);
    }

    public ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
    {
        if (_memory != null)
        {
            return new(_memory.Value);
        }

        var pending = Framing.ReadToEndAsync(CancellationToken.None);

        if (pending.IsCompletedSuccessfully)
        {
            var memory = pending.Result;

            _memory = memory;

            return new(memory);
        }

        return RememberAsync(pending);
    }

    private async ValueTask<ReadOnlyMemory<byte>> RememberAsync(ValueTask<ReadOnlyMemory<byte>> pending)
    {
        var memory = await pending;

        _memory = memory;

        return memory;
    }

    #endregion

}
