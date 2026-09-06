namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// The protocols supported by a port binding.
/// </summary>
[Flags]
public enum HttpProtocols
{
    
    None = 0,
    
    Http1 = 1,

    Http2 = 2,

    Http3 = 4,

    Http1AndHttp2 = Http1 | Http2,

    Http1AndHttp3 = Http1 | Http3,

    Http2AndHttp3 = Http2 | Http3,

    All = Http1 | Http2 | Http3
    
}
