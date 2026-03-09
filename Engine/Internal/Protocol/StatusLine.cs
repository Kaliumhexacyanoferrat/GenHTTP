using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Internal.Protocol;

internal static class StatusLine
{
    private static readonly Dictionary<ResponseStatus, ReadOnlyMemory<byte>> Lines = new()
    {
        { ResponseStatus.Ok, "HTTP/1.1 200 OK\r\n"u8.ToArray() },
        { ResponseStatus.NoContent, "HTTP/1.1 204 No Content\r\n"u8.ToArray() },
        { ResponseStatus.NotFound, "HTTP/1.1 404 Not Found\r\n"u8.ToArray() },
        { ResponseStatus.InternalServerError, "HTTP/1.1 500 Internal Server Error\r\n"u8.ToArray() }
    };

    internal static ReadOnlyMemory<byte> Get(ResponseStatus status) => Lines[status];
    
}
