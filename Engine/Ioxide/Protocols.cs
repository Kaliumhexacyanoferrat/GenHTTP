namespace GenHTTP.Engine.Ioxide;

/// <summary>Which HTTP versions a port serves.</summary>
[Flags]
public enum Protocols
{
    Http1 = 1,

    Http2 = 2,

    Http3 = 4,

    Http1AndHttp2 = Http1 | Http2,

    Http1AndHttp3 = Http1 | Http3,

    Http2AndHttp3 = Http2 | Http3,

    All = Http1 | Http2 | Http3,
}
