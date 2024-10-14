using System.Text;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Reflection;

public static partial class PathArguments
{
    private static readonly MethodRouting Empty = new("^(/|)$", true, false);

    private static readonly MethodRouting EmptyWildcard = new("^.*", true, true);

    private static readonly Regex VarPattern = CreateVarPattern();

    [GeneratedRegex(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateVarPattern();

    /// <summary>
    /// Parses the given path and returns a routing structure
    /// expected by the method provider to check which logic
    /// to be executed on request.
    /// </summary>
    /// <param name="path">The path to be analyzed</param>
    /// <param name="wildcard">If true, a route will be created that matches any sub path</param>
    /// <returns>The routing information to be used by the method provider</returns>
    public static MethodRouting Route(string? path, bool wildcard = false)
    {
        if (path is not null)
        {
            var builder = new StringBuilder(path);

            if (builder.Length > 0 && builder[0] == '/')
            {
                builder.Remove(0, 1);
            }

            if (builder.Length > 0 && builder[^1] == '/')
            {
                builder.Remove(builder.Length - 1, 1);
            }

            // convert parameters of the format ":var" into appropriate groups
            foreach (Match match in VarPattern.Matches(path))
            {
                builder.Replace(match.Value, match.Groups[1].Value.ToParameter());
            }

            var end = wildcard ? "(/|)" : "(/|)$";

            return new MethodRouting($"^/{builder}{end}", false, wildcard);
        }

        return wildcard ? EmptyWildcard : Empty;
    }

    /// <summary>
    /// Checks, whether the given type ultimately returns a handler or handler builder,
    /// so requests should passed to this handler which means that we allow any sub
    /// routes here.
    /// </summary>
    /// <param name="returnType">The return type to be checked</param>
    /// <returns>true, if the given type will ultimately create an IHandler instance that should handle the request</returns>
    public static bool CheckWildcardRoute(Type returnType)
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

}
