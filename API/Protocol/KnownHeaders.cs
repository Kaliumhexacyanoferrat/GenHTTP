namespace GenHTTP.Api.Protocol;

public static class KnownHeaders
{
    
    public static readonly ByteString Connection = new("Connection"u8.ToArray());
    
    public static readonly ByteString Server = new("Server"u8.ToArray());

    public static readonly ByteString Date = new("Date"u8.ToArray());
    
    public static readonly ByteString Host = new("Host"u8.ToArray());

    public static readonly ByteString Vary = new("Vary"u8.ToArray());
    
    public static readonly ByteString ContentType = new("Content-Type"u8.ToArray());

    public static readonly ByteString ContentEncoding = new("Content-Encoding"u8.ToArray());

    public static readonly ByteString ContentLength = new("Content-Length"u8.ToArray());

    public static readonly ByteString TransferEncoding = new("Transfer-Encoding"u8.ToArray());
    
    public static readonly ByteString Accept = new("Accept"u8.ToArray());
    
    public static readonly ByteString AcceptEncoding = new("Accept-Encoding"u8.ToArray());

    public static readonly ByteString Expires = new("Expires"u8.ToArray());

    public static readonly ByteString IfNoneMatch = new("If-None-Match"u8.ToArray());
        
    public static readonly ByteString ETag = new("ETag"u8.ToArray());

}
