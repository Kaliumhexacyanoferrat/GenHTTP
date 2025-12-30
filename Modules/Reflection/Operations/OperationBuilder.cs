using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection.Routing;
using GenHTTP.Modules.Reflection.Routing.Segments;

namespace GenHTTP.Modules.Reflection.Operations;

public static class OperationBuilder
{
    private static readonly OperationRoute IndexRoute = new("/", [new ClosingSegment(false, false)], false);

    private static readonly OperationRoute WildcardIndexRoute = new("/", [new ClosingSegment(false, true)], true);

    #region Functionality

    /// <summary>
    /// Analyzes the given configuration and converts it into an operation that can
    /// get executed by the <see cref="MethodHandler" />.
    /// </summary>
    /// <param name="definition">The path definition of the endpoint, e.g. "/users/:id"</param>
    /// <param name="method">The actual .NET method to be executed to retrieve a result</param>
    /// <param name="del">If the method is defined by a delegate and not as an instance method, pass it here</param>
    /// <param name="registry">The customizable registry used to read and write data</param>
    /// <param name="forceTrailingSlash">If set to true, the operation requires the client to append a trailing slash to the path</param>
    /// <returns>The newly created operation</returns>
    public static Operation Create(IRequest request, string? definition, MethodInfo method, Delegate? del, ExecutionSettings executionSettings, IMethodConfiguration configuration, MethodRegistry registry, bool forceTrailingSlash = false)
    {
        var isWildcard = CheckWildcardRoute(method.ReturnType);

        OperationRoute route;

        if (string.IsNullOrWhiteSpace(definition))
        {
            route = isWildcard ? WildcardIndexRoute : IndexRoute;
        }
        else
        {
            var normalized = Normalize(definition);

            var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var segments = new List<IRoutingSegment>(parts.Length + 1);

            foreach (var part in parts)
            {
                var colonCount = part.Count(c => c == ':');

                if (part.StartsWith(':') && IsValidVariable(part.AsSpan()[1..]) && colonCount == 1)
                {
                    segments.Add(new SimpleVariableSegment(part[1..]));
                }
                else if (part.Contains("?<") || colonCount > 0)
                {
                    segments.Add(new RegexSegment(part));
                }
                else
                {
                    segments.Add(new StringSegment(part));
                }
            }

            segments.Add(new ClosingSegment(forceTrailingSlash || definition.EndsWith('/'), isWildcard));

            var displayName = GetDisplayName(definition, forceTrailingSlash);

            route = new OperationRoute(displayName, segments, isWildcard);
        }
        
        var pathArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in route.Segments)
        {
            foreach (var pathArg in segment.ProvidedArguments)
            {
                pathArguments.Add(pathArg);
            }
        }

        var arguments = SignatureAnalyzer.GetArguments(request, method, pathArguments, registry);

        var result = SignatureAnalyzer.GetResult(method, registry);

        var interceptors = InterceptorAnalyzer.GetInterceptors(method);

        return new Operation(method, del, executionSettings, configuration, route, result, arguments, interceptors);
    }

    private static bool CheckWildcardRoute(Type returnType)
    {
        if (IsHandlerType(returnType))
        {
            return true;
        }

        if (returnType.IsAsyncGeneric())
        {
            if (returnType.GenericTypeArguments.Length == 1)
            {
                return IsHandlerType(returnType.GenericTypeArguments[0]);
            }
        }

        return false;
    }

    private static bool IsHandlerType(Type returnType) => typeof(IHandlerBuilder).IsAssignableFrom(returnType) || typeof(IHandler).IsAssignableFrom(returnType);

    private static string GetDisplayName(string definition, bool forceTrailingSlash)
    {
        var nameBuilder = new StringBuilder(Normalize(definition));

        ReplaceMatches(nameBuilder, definition, ":([A-Za-z0-9]+)"); // :var

        ReplaceMatches(nameBuilder, definition, @"\(\?\<([a-z]+)\>([^)]+)\)"); // (?<ean13>[0-9]{12,13})

        if (forceTrailingSlash || definition.EndsWith('/'))
        {
            nameBuilder.Append('/');
        }

        if (nameBuilder.Length > 1)
        {
            nameBuilder.Insert(0, '/');
        }

        return nameBuilder.ToString();
    }

    private static void ReplaceMatches(StringBuilder sb, string definition, string regex)
    {
        var pattern = new Regex(regex);

        var matches = pattern.Matches(definition);

        foreach (Match match in matches)
        {
            sb.Replace(match.Value, $"{{{match.Groups[1].Value}}}");
        }
    }

    private static string Normalize(string definition)
    {
        int trimStart = 0, trimEnd = 0;

        if (definition.Length > 0)
        {
            if (definition[0] == '/')
            {
                trimStart = 1;
            }

            if (definition[^1] == '/')
            {
                trimEnd = 1;
            }
        }

        return definition.Substring(trimStart, definition.Length - trimStart - trimEnd);
    }

    private static bool IsValidVariable(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (c is (< 'A' or > 'Z') and (< 'a' or > 'z') and (< '0' or > '9')) 
            {
                return false;
            }
        }

        return true;
    }

    #endregion

}
