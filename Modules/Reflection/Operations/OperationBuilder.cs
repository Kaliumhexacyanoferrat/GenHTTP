using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Reflection.Operations;

public static partial class OperationBuilder
{
    private static readonly Regex VarPattern = CreateVarPattern();

    private static readonly Regex EmptyWildcardRoute = CreateEmptyWildcardRoute();

    private static readonly Regex EmptyRoute = CreateEmptyRoute();

    #region Functionality

    public static Operation Create(string? definition, MethodInfo method)
    {
        var isWildcard = CheckWildcardRoute(method.ReturnType);

        OperationPath path;

        Dictionary<string, OperationArgument> pathArguments = [];

        if (string.IsNullOrWhiteSpace(definition))
        {
            if (isWildcard)
            {
                pathArguments.Add("wildcard", new OperationArgument("wildcard", OperationArgumentSource.Path));
                path = new OperationPath("/{wildcard}", EmptyWildcardRoute, true, true);
            }
            else
            {
                path = new OperationPath("/", EmptyRoute, true, false);
            }
        }
        else
        {
            var normalized = Normalize(definition);

            var matchBuilder = new StringBuilder(normalized);
            var nameBuilder = new StringBuilder(WithPrefix(normalized));

            // convert parameters of the format ":var" into appropriate groups
            foreach (Match match in VarPattern.Matches(definition))
            {
                matchBuilder.Replace(match.Value, match.Groups[1].Value.ToParameter());
                nameBuilder.Replace(match.Value, "{" + match.Groups[1].Value + "}");
            }

            var end = isWildcard ? "(/|)" : "(/|)$";

            var matcher = new Regex($"^/{matchBuilder}{end}", RegexOptions.Compiled);

            path = new OperationPath(nameBuilder.ToString(), matcher, false, isWildcard);
        }

        return new Operation(path, AnalyzeArguments(path, pathArguments, method));
    }

    private static IReadOnlyDictionary<string, OperationArgument> AnalyzeArguments(OperationPath path, Dictionary<string, OperationArgument> pathArguments, MethodInfo method)
    {

        return pathArguments;
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

    private static string WithPrefix(string path)
    {
        if (path.Length > 0)
        {
            if (path[0] != '/')
            {
                return $"{path}";
            }
        }

        return path;
    }

    #endregion

    #region Regular Expressions

    [GeneratedRegex(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateVarPattern();

    [GeneratedRegex("^.*", RegexOptions.Compiled)]
    private static partial Regex CreateEmptyWildcardRoute();

    [GeneratedRegex("^(/|)$", RegexOptions.Compiled)]
    private static partial Regex CreateEmptyRoute();

    #endregion

}
