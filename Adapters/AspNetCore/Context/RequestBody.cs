using GenHTTP.Api.Protocol;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Grants access to the body of a Kestrel-hosted request.
/// </summary>
/// <remarks>
/// Considerably simpler than the Internal/Ioxide engines' body types: Kestrel has
/// already de-chunked the request body by the time it reaches us, so there is no
/// need to replicate their <c>ConsumptionStrategy</c>/<c>ChunkedBodyStream</c>
/// machinery (which expects raw, wire-framed bytes) - we just read the body stream
/// Kestrel already hands us.
/// </remarks>
internal sealed class RequestBody : IRequestBody
{
    private Stream? _stream;

    private ReadOnlyMemory<byte>? _memory;

    private bool _opened;

    #region Get-/Setters

    public ulong? Length { get; private set; }

    private Stream Stream => _stream ?? throw new InvalidOperationException("Body has not been initialized");

    #endregion

    #region Initialization

    public void Apply(Stream body, ulong? length)
    {
        _stream = body;
        Length = length;
    }

    public void Reset()
    {
        _stream = null;
        Length = null;
        _memory = null;
        _opened = false;
    }

    #endregion

    #region Functionality

    public Stream AsStream()
    {
        EnsureNotYetOpened();

        return Stream;
    }

    public async ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
    {
        if (_memory is { } cached)
        {
            return cached;
        }

        EnsureNotYetOpened();

        var stream = Stream;

        if (Length is { } length && length <= int.MaxValue)
        {
            var buffer = GC.AllocateUninitializedArray<byte>((int)length);

            var read = 0;

            while (read < buffer.Length)
            {
                var count = await stream.ReadAsync(buffer.AsMemory(read));

                if (count == 0)
                {
                    break;
                }

                read += count;
            }

            return (_memory = buffer.AsMemory(0, read)).Value;
        }

        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        return (_memory = memoryStream.ToArray()).Value;
    }

    private void EnsureNotYetOpened()
    {
        if (_opened)
        {
            throw new InvalidOperationException("The body of the request has already been opened");
        }

        _opened = true;
    }

    #endregion

}
