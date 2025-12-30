namespace GenHTTP.Modules.Reflection.Routing;

/// <summary>
/// Returned by the operation router if a web service method matched
/// the incoming request.
/// </summary>
/// <param name="Offset">The offset to advance the request target by</param>
/// <param name="PathArguments">The arguments read from the request path, if any</param>
public record RoutingMatch(int Offset, IReadOnlyDictionary<string, string>? PathArguments);
