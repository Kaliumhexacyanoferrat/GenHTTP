using System;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Reflection;

public static class PathArguments
{
    private static readonly MethodRouting EMPTY = new("/", "^(/|)$", null, true, false);

    private static readonly MethodRouting EMPTY_WILDCARD = new("/", "^.*", null, true, true);

    private static readonly Regex VAR_PATTERN = new(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

                if (builder[0] == '/')
                {
                    builder.Remove(0, 1);
                }

                if (builder.Length > 0 && builder[^1] == '/')
                {
                    builder.Remove(builder.Length - 1, 1);
                }

                // convert parameters of the format ":var" into appropriate groups
                foreach (Match match in VAR_PATTERN.Matches(path))
                {
                    builder.Replace(match.Value, match.Groups[1].Value.ToParameter());
                }

                var splitted = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var end = (wildcard) ? "(/|)" : "(/|)$";

                return new MethodRouting(path, $"^/{builder}{end}", (splitted.Length > 0) ? splitted[0] : null, false, wildcard);
            }

            return (wildcard) ? EMPTY_WILDCARD : EMPTY;
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

    private static bool IsHandlerType(Type returnType)
    {
            return typeof(IHandlerBuilder).IsAssignableFrom(returnType) || typeof(IHandler).IsAssignableFrom(returnType);
        }

}
