using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection.Routing.Segments;

internal partial class RegexSegment : IRoutingSegment
{
    private static readonly Regex VarPattern = CreateVarPattern();

    private static readonly Regex RegexPattern = CreateRegexPattern();

    private readonly Regex _matcher;

    public string[] ProvidedArguments { get; }

    public RegexSegment(string definition)
    {
        var providedArguments = new List<string>();

        var matchBuilder = new StringBuilder(definition);

        // convert parameters of the format ":var" into appropriate groups
        foreach (Match match in VarPattern.Matches(definition))
        {
            var name = match.Groups[1].Value;

            matchBuilder.Replace(match.Value, name.ToParameter());

            providedArguments.Add(name);
        }

        // convert advanced regex params as well
        foreach (Match match in RegexPattern.Matches(definition))
        {
            var name = match.Groups[1].Value;

            providedArguments.Add(name);
        }

        _matcher = new Regex($"^{matchBuilder}$", RegexOptions.Compiled);

        ProvidedArguments = providedArguments.ToArray();
    }

    public (bool matched, int offsetBy) TryMatch(RoutingTarget target, int offset, ref PathArgumentSink argumentSink)
    {
        var part = target.Next(offset);

        if (part is null)
        {
            return (false, 0);
        }

        var match = _matcher.Match(part.Value);

        if (!match.Success)
        {
            return (false, 0);
        }

        var groups = match.Groups;
            
        for (var i = 1; i < groups.Count; i++)
        {
            argumentSink.Add(groups[i].Name, groups[i].Value);
        }
            
        return (true, 1);
    }

    [GeneratedRegex(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateVarPattern();

    [GeneratedRegex(@"\(\?\<([a-z]+)\>([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateRegexPattern();

}
