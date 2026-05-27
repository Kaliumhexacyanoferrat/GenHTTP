namespace GenHTTP.Api.Protocol;

public static class KnownHeaders
{
    
    public static readonly ReadOnlyMemory<byte> Server = "Server"u8.ToArray();

    public static readonly ReadOnlyMemory<byte> Date = "Date"u8.ToArray();
    
    public static readonly ReadOnlyMemory<byte> Host = "Host"u8.ToArray();

    public static readonly ReadOnlyMemory<byte> ContentType = "Content-Type"u8.ToArray();

    public static readonly ReadOnlyMemory<byte> ContentEncoding = "Content-Encoding"u8.ToArray();

    public static readonly ReadOnlyMemory<byte> ContentLength = "Content-Length"u8.ToArray();

    public static readonly ReadOnlyMemory<byte> TransferEncoding = "Transfer-Encoding"u8.ToArray();

}
