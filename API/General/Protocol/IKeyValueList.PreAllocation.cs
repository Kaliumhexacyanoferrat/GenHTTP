using System.Runtime.CompilerServices;

namespace GenHTTP.Api.Protocol;

public static class IKeyValueListPreAllocationExtensions
{

    /// <summary>
    /// Allocates the backing memory of the given byte string
    /// if needed, so that the value will stay available after
    /// a handler has started to read the request body.
    /// </summary>
    /// <param name="byteString">The string to allocate (if applicable)</param>
    /// <param name="request">The request the string has been read from</param>
    /// <returns>The (maybe) pre-allocated byte string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ByteString? PreAllocate(this ByteString? byteString, IRequest request)
        => ((byteString != null) && request.HasBody) ? new(byteString.Value.Bytes.ToArray()) : byteString;

}
