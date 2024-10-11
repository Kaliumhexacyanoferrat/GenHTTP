﻿using System.Reflection;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Controllers.Provider;

public sealed partial class ControllerHandler : IHandler
{
    private static readonly MethodRouting Empty = new("^(/|)$", true, false);

    private static readonly Regex HyphenMatcher = CreateHyphenMatcher();

    #region Get-/Setters

    public IHandler Parent { get; }

    private MethodCollection Provider { get; }

    private ResponseProvider ResponseProvider { get; }

    private FormatterRegistry Formatting { get; }

    private object Instance { get; }

    #endregion

    #region Initialization

    public ControllerHandler(IHandler parent, object instance, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
    {
        Parent = parent;
        Formatting = formatting;

        Instance = instance;

        ResponseProvider = new ResponseProvider(serialization, formatting);

        Provider = new MethodCollection(this, AnalyzeMethods(instance.GetType(), serialization, injection, formatting));
    }

    private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var annotation = method.GetCustomAttribute<ControllerActionAttribute>(true) ?? new MethodAttribute();

            var arguments = FindPathArguments(method);

            var path = DeterminePath(method, arguments);

            yield return parent => new MethodHandler(parent, method, path, () => Instance, annotation, ResponseProvider.GetResponseAsync, serialization, injection, formatting);
        }
    }

    private static MethodRouting DeterminePath(MethodInfo method, List<string> arguments)
    {
        var pathArgs = string.Join('/', arguments.Select(a => a.ToParameter()));

        var isWildcard = PathArguments.CheckWildcardRoute(method.ReturnType);

        if (method.Name == "Index")
        {
            return pathArgs.Length > 0 ? new MethodRouting($"^/{pathArgs}/", false, isWildcard) : Empty;
        }

        var name = HypenCase(method.Name);

        var path = $"^/{name}";

        return pathArgs.Length > 0 ? new MethodRouting( $"{path}/{pathArgs}/", false, isWildcard) : new MethodRouting($"{path}/", false, isWildcard);
    }

    private List<string> FindPathArguments(MethodInfo method)
    {
        var found = new List<string>();

        var parameters = method.GetParameters();

        foreach (var parameter in parameters)
        {
            if (parameter.GetCustomAttribute<FromPathAttribute>(true) is not null)
            {
                if (!parameter.CanFormat(Formatting))
                {
                    throw new InvalidOperationException("Parameters marked as 'FromPath' must be formattable (e.g. string or int)");
                }

                if (parameter.CheckNullable())
                {
                    throw new InvalidOperationException("Parameters marked as 'FromPath' are not allowed to be nullable");
                }

                if (parameter.Name is null)
                {
                    throw new InvalidOperationException("Parameters marked as 'FromPath' must have a name");
                }

                found.Add(parameter.Name);
            }
        }

        return found;
    }

    private static string HypenCase(string input) => HyphenMatcher.Replace(input, "$1-$2").ToLowerInvariant();

    [GeneratedRegex("([a-z])([A-Z0-9]+)")]
    private static partial Regex CreateHyphenMatcher();

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Provider.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Provider.HandleAsync(request);

    #endregion

}
