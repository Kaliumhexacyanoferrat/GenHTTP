﻿using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerCaching;

public record CachedResponse(int StatusCode, string StatusPhrase, DateTime? Expires, DateTime? Modified,
    Dictionary<string, string>? Variations, Dictionary<string, string> Headers,
    Dictionary<string, Cookie>? Cookies, string? ContentType,
    string? ContentEncoding, ulong? ContentLength, ulong? ContentChecksum);
