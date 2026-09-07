namespace GenHTTP.Api.Protocol;

/// <summary>
/// An ASCII string read from the request buffer without
/// specific meaning, e.g. a header or query value.
/// </summary>
[MemoryView]
public readonly partial struct ByteString;
