namespace GenHTTP.Modules.Reflection.Routing;

public record RoutingMatch(int Offset, IReadOnlyDictionary<string, string>? PathArguments);
