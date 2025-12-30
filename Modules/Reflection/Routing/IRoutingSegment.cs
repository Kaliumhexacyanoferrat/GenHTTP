using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing;

public interface IRoutingSegment
{
    
    string[] ProvidedArguments { get; }

    (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, Action<string, string> pathParamCallback);
    
}
