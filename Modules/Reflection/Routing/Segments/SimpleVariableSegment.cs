using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing.Segments;

/// <summary>
/// Matches a single variable, such as "/:var/".
/// </summary>
/// <param name="variableName">The name of the variable to look for (without the colon)</param>
/// <remarks>
/// This implementation allows a simple path variable (which is the common use case)
/// without the need of creating a regex for parsing.
/// </remarks>
internal sealed class SimpleVariableSegment(string variableName) : IRoutingSegment
{

    public string[] ProvidedArguments { get; } = [variableName];
    
    public (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, ref PathArgumentSink argumentSink)
    {
        var part = target.Next(offset);

        if (part is null)
        {
            return (false, 0);
        }

        argumentSink.Add(variableName, part.Value);
        return (true, 1);
    }
    
}
