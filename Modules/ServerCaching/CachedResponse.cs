namespace GenHTTP.Modules.ServerCaching;

public record CachedResponse(int StatusCode /* todo */,
    Dictionary<string, string>? Variations, Dictionary<string, string> Headers,
    string? ContentType, string? ContentEncoding, ulong? ContentLength, ulong? ContentChecksum);
