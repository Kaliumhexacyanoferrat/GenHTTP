using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;

namespace GenHTTP.Modules.ServerCaching
{
    
    public record CachedResponse
    (

        FlexibleResponseStatus Status,

        DateTime? Expires,

        DateTime? Modified,

        List<string>? Variations,

        Dictionary<string, string> Headers,

        Dictionary<string, Cookie>? Cookies,

        FlexibleContentType? ContentType,

        string? ContentEncoding,

        ulong? ContentLength,

        ulong? ContentChecksum

    );

}
