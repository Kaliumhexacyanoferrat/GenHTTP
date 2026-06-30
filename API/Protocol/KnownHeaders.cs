namespace GenHTTP.Api.Protocol;

/// <summary>
/// Header names that are shared across several handlers and concerns.
/// </summary>
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

    public static readonly ByteString Cookie = new("Cookie");

    public static readonly ByteString SetCookie = new("Set-Cookie");

    public static readonly ByteString Forwarded = new("Forwarded");

    public static readonly ByteString ForwardedFor = new("X-Forwarded-For");

    public static readonly ByteString ForwardedHost = new("X-Forwarded-Host");

    public static readonly ByteString ForwardedProto = new("X-Forwarded-Proto");

}
