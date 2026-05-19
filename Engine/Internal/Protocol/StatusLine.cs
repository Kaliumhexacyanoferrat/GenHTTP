using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Internal.Protocol;

internal static class StatusLine
{
    private static readonly byte[][] Lines = Init();

    public static byte[] Get(ResponseStatus status) => Lines[(int)status];

    private static byte[][] Init()
    {
        var arr = new byte[600][];

        // 1xx
        arr[100] = "HTTP/1.1 100 Continue\r\n"u8.ToArray();
        arr[101] = "HTTP/1.1 101 Switching Protocols\r\n"u8.ToArray();
        arr[102] = "HTTP/1.1 102 Processing\r\n"u8.ToArray(); // WebDAV

        // 2xx
        arr[200] = "HTTP/1.1 200 OK\r\n"u8.ToArray();
        arr[201] = "HTTP/1.1 201 Created\r\n"u8.ToArray();
        arr[202] = "HTTP/1.1 202 Accepted\r\n"u8.ToArray();
        arr[203] = "HTTP/1.1 203 Non-Authoritative Information\r\n"u8.ToArray();
        arr[204] = "HTTP/1.1 204 No Content\r\n"u8.ToArray();
        arr[205] = "HTTP/1.1 205 Reset Content\r\n"u8.ToArray();
        arr[206] = "HTTP/1.1 206 Partial Content\r\n"u8.ToArray();
        arr[207] = "HTTP/1.1 207 Multi-Status\r\n"u8.ToArray();  // WebDAV
        arr[208] = "HTTP/1.1 208 Already Reported\r\n"u8.ToArray(); // WebDAV
        arr[226] = "HTTP/1.1 226 IM Used\r\n"u8.ToArray();       // Delta encoding

        // 3xx
        arr[300] = "HTTP/1.1 300 Multiple Choices\r\n"u8.ToArray();
        arr[301] = "HTTP/1.1 301 Moved Permanently\r\n"u8.ToArray();
        arr[302] = "HTTP/1.1 302 Found\r\n"u8.ToArray();
        arr[303] = "HTTP/1.1 303 See Other\r\n"u8.ToArray();
        arr[304] = "HTTP/1.1 304 Not Modified\r\n"u8.ToArray();
        arr[305] = "HTTP/1.1 305 Use Proxy\r\n"u8.ToArray();
        arr[307] = "HTTP/1.1 307 Temporary Redirect\r\n"u8.ToArray();
        arr[308] = "HTTP/1.1 308 Permanent Redirect\r\n"u8.ToArray();

        // 4xx
        arr[400] = "HTTP/1.1 400 Bad Request\r\n"u8.ToArray();
        arr[401] = "HTTP/1.1 401 Unauthorized\r\n"u8.ToArray();
        arr[402] = "HTTP/1.1 402 Payment Required\r\n"u8.ToArray();
        arr[403] = "HTTP/1.1 403 Forbidden\r\n"u8.ToArray();
        arr[404] = "HTTP/1.1 404 Not Found\r\n"u8.ToArray();
        arr[405] = "HTTP/1.1 405 Method Not Allowed\r\n"u8.ToArray();
        arr[406] = "HTTP/1.1 406 Not Acceptable\r\n"u8.ToArray();
        arr[407] = "HTTP/1.1 407 Proxy Authentication Required\r\n"u8.ToArray();
        arr[408] = "HTTP/1.1 408 Request Timeout\r\n"u8.ToArray();
        arr[409] = "HTTP/1.1 409 Conflict\r\n"u8.ToArray();
        arr[410] = "HTTP/1.1 410 Gone\r\n"u8.ToArray();
        arr[411] = "HTTP/1.1 411 Length Required\r\n"u8.ToArray();
        arr[412] = "HTTP/1.1 412 Precondition Failed\r\n"u8.ToArray();
        arr[413] = "HTTP/1.1 413 Payload Too Large\r\n"u8.ToArray();
        arr[414] = "HTTP/1.1 414 URI Too Long\r\n"u8.ToArray();
        arr[415] = "HTTP/1.1 415 Unsupported Media Type\r\n"u8.ToArray();
        arr[416] = "HTTP/1.1 416 Range Not Satisfiable\r\n"u8.ToArray();
        arr[417] = "HTTP/1.1 417 Expectation Failed\r\n"u8.ToArray();
        arr[418] = "HTTP/1.1 418 I'm a Teapot\r\n"u8.ToArray();
        arr[421] = "HTTP/1.1 421 Misdirected Request\r\n"u8.ToArray();
        arr[422] = "HTTP/1.1 422 Unprocessable Entity\r\n"u8.ToArray();
        arr[423] = "HTTP/1.1 423 Locked\r\n"u8.ToArray();
        arr[424] = "HTTP/1.1 424 Failed Dependency\r\n"u8.ToArray();
        arr[425] = "HTTP/1.1 425 Too Early\r\n"u8.ToArray();
        arr[426] = "HTTP/1.1 426 Upgrade Required\r\n"u8.ToArray();
        arr[428] = "HTTP/1.1 428 Precondition Required\r\n"u8.ToArray();
        arr[429] = "HTTP/1.1 429 Too Many Requests\r\n"u8.ToArray();
        arr[431] = "HTTP/1.1 431 Request Header Fields Too Large\r\n"u8.ToArray();
        arr[451] = "HTTP/1.1 451 Unavailable For Legal Reasons\r\n"u8.ToArray();

        // 5xx
        arr[500] = "HTTP/1.1 500 Internal Server Error\r\n"u8.ToArray();
        arr[501] = "HTTP/1.1 501 Not Implemented\r\n"u8.ToArray();
        arr[502] = "HTTP/1.1 502 Bad Gateway\r\n"u8.ToArray();
        arr[503] = "HTTP/1.1 503 Service Unavailable\r\n"u8.ToArray();
        arr[504] = "HTTP/1.1 504 Gateway Timeout\r\n"u8.ToArray();
        arr[505] = "HTTP/1.1 505 HTTP Version Not Supported\r\n"u8.ToArray();
        arr[506] = "HTTP/1.1 506 Variant Also Negotiates\r\n"u8.ToArray();
        arr[507] = "HTTP/1.1 507 Insufficient Storage\r\n"u8.ToArray();
        arr[508] = "HTTP/1.1 508 Loop Detected\r\n"u8.ToArray();
        arr[510] = "HTTP/1.1 510 Not Extended\r\n"u8.ToArray();
        arr[511] = "HTTP/1.1 511 Network Authentication Required\r\n"u8.ToArray();

        return arr;
    }
    
}
