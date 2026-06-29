using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerCaching;

public record CachedResponse(ResponseStatus Status,
    Dictionary<string, string>? Variations, Dictionary<string, string> Headers,
    string? ContentType, string? ContentEncoding, ulong? ContentLength, ulong? ContentChecksum);
