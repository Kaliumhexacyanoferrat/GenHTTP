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

    private readonly ContinueReader _continue = new();

    private PipeReader? _reader;

    private PipeWriter? _continueWriter;

    private SequencePosition _start;

    private BodyState _state;

    private bool _present;

    private int _contentLengthIndex = -1;

    private int _transferEncodingIndex = -1;

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

    private PipeReader BodyReader => (_continueWriter != null) ? _continue : Reader;

    private IBodyFraming Framing => _framing ?? throw new InvalidOperationException("Request body has not been opened");

    #endregion

    #region Initialization

    internal RequestBody(RequestHeader header)
    {
        _header = header;
    }

    /// <param name="writer">The writer of the connection, used to send interim responses (if supported)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Apply(PipeReader reader, SequencePosition start, PipeWriter? writer)
    {
        _reader = reader;
        _start = start;
        _state = BodyState.Pending;

        _wrapper = null;
        _wrapped = null;

        var expectation = ScanHeaders();

        _present = _contentLengthIndex >= 0 || _transferEncodingIndex >= 0;

        if (_present && writer != null && expectation >= 0 && ExpectsContinue(expectation))
        {
            _continueWriter = writer;
            _continue.Arm(reader, writer);
        }
    }

    internal void Apply() => Clear();

    /// <summary>
    /// Finds the headers that describe the body in a single pass.
    /// </summary>
    /// <returns>The index of the Expect header, or -1 if there is none</returns>
    private int ScanHeaders()
    {
        var headers = _header.Headers;

        int contentLength = -1, transferEncoding = -1, expectation = -1;

        for (var i = 0; i < headers.Count; i++)
        {
            var key = headers.GetMemoryEntry(i).Key;

            if (contentLength < 0 && key == KnownHeaders.ContentLength)
            {
                contentLength = i;
            }
            else if (transferEncoding < 0 && key == KnownHeaders.TransferEncoding)
            {
                transferEncoding = i;
            }
            else if (expectation < 0 && key == KnownHeaders.Expect)
            {
                expectation = i;
            }
        }

        _contentLengthIndex = contentLength;
        _transferEncodingIndex = transferEncoding;

        return expectation;
    }

    /// <summary>
    /// Checks whether the client waits for a 100 (Continue) before sending the body.
    /// </summary>
    /// <remarks>
    /// The expectation is ignored for HTTP/1.0 clients (RFC 9110, section 10.1.1).
    /// </remarks>
    private bool ExpectsContinue(int index)
        => _header.Protocol != HttpProtocol.Http10 && ContinueReader.IsContinue(HeaderValue(index));

    private ByteString HeaderValue(int index) => new(_header.Headers.GetMemoryEntry(index).Value);

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

        _contentLengthIndex = -1;
        _transferEncodingIndex = -1;

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

        if (_continueWriter != null)
        {
            _continue.Reset();
            _continueWriter = null;
        }
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
    /// Invoked right before the final response is written to the connection.
    /// </summary>
    /// <remarks>
    /// From here on, reading the body no longer sends a 100 (Continue), as no
    /// interim response may follow the final one.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResponseStarting()
    {
        if (_continueWriter != null)
        {
            _continue.Disarm();
        }
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

        if (framing == null)
        {
            return new(false);
        }

        var continueWriter = _continueWriter;

        if (continueWriter != null)
        {
            return FinishExpectedAsync(framing, continueWriter);
        }

        return framing.SkipAsync();
    }

    /// <summary>
    /// Skips the body of a client that expected a 100 (Continue).
    /// </summary>
    /// <remarks>
    /// If the 100 has never been sent, the client holds its body back until it
    /// hears from the server, so the final response needs to be on the wire before
    /// the client will either send the body or close the connection.
    /// </remarks>
    private async ValueTask<bool> FinishExpectedAsync(IBodyFraming framing, PipeWriter writer)
    {
        _continue.Disarm();

        if (!_continue.Sent)
        {
            await writer.FlushAsync();
        }

        return await framing.SkipAsync();
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
        if (_contentLengthIndex >= 0)
        {
            if (!ulong.TryParse(HeaderValue(_contentLengthIndex).Bytes.Span, out var length) || length > long.MaxValue)
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Content-Length header has an invalid value");
            }

            Length = length;

            _contentLength.Apply(BodyReader, (long)length);

            return _framing = _contentLength;
        }

        if (_transferEncodingIndex >= 0 && HeaderValue(_transferEncodingIndex).Bytes.Span.SequenceEqual(ChunkedValue.Span))
        {
            _chunked.Apply(BodyReader);

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
