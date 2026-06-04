namespace GenHTTP.Api.Protocol;

public static class KnownHeaders
{
    
    public static readonly ByteString Connection = new("Connection");
    
    public static readonly ByteString Server = new("Server");

    public static readonly ByteString Date = new("Date");
    
    public static readonly ByteString Host = new("Host");

    public static readonly ByteString Vary = new("Vary");
    
    public static readonly ByteString ContentType = new("Content-Type");

    public static readonly ByteString ContentEncoding = new("Content-Encoding");

    public static readonly ByteString ContentLength = new("Content-Length");

    public static readonly ByteString TransferEncoding = new("Transfer-Encoding");
    
    public static readonly ByteString Accept = new("Accept");
    
    public static readonly ByteString AcceptEncoding = new("Accept-Encoding");

    public static readonly ByteString Expires = new("Expires");

    public static readonly ByteString IfNoneMatch = new("If-None-Match");
        
    public static readonly ByteString ETag = new("ETag");

}
