﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Reflection.Operations;

public static partial class OperationBuilder
{
    private static readonly Regex VarPattern = CreateVarPattern();

    private static readonly Regex RegexPattern = CreateRegexPattern();

    private static readonly Regex EmptyWildcardRoute = CreateEmptyWildcardRoute();

    private static readonly Regex EmptyRoute = CreateEmptyRoute();

    #region Functionality

    public static Operation Create(string? definition, MethodInfo method, MethodRegistry registry, bool forceTrailingSlash = false)
    {
        var isWildcard = CheckWildcardRoute(method.ReturnType);

        OperationPath path;

        var pathArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(definition))
        {
            if (isWildcard)
            {
                // ToDo: document the injection possibilities
                path = new OperationPath("/{path}", EmptyWildcardRoute, true, true);
                pathArguments.Add("path");
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
                var name = match.Groups[1].Value;

                matchBuilder.Replace(match.Value, name.ToParameter());
                nameBuilder.Replace(match.Value, "{" + name + "}");

                pathArguments.Add(name);
            }

            // convert advanced regex params as well
            foreach (Match match in RegexPattern.Matches(definition))
            {
                var name = match.Groups[1].Value;

                nameBuilder.Replace(match.Value, "{" + name + "}");

                pathArguments.Add(name);
            }

            var end = forceTrailingSlash ? "/" : "(/|)";
            end = isWildcard ? end : $"{end}$";

            var matcher = new Regex($"^/{matchBuilder}{end}", RegexOptions.Compiled);

            path = new OperationPath(nameBuilder.ToString(), matcher, false, isWildcard);
        }

        var arguments = SignatureAnalyzer.GetArguments(method, pathArguments, registry);

        var result = SignatureAnalyzer.GetResult(method, registry);

        return new Operation(method, path, result, arguments);
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

    [GeneratedRegex(@"\(\?\<([a-z]+)\>([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateRegexPattern();

    [GeneratedRegex("^.*", RegexOptions.Compiled)]
    private static partial Regex CreateEmptyWildcardRoute();

    [GeneratedRegex("^(/|)$", RegexOptions.Compiled)]
    private static partial Regex CreateEmptyRoute();

    #endregion

}
