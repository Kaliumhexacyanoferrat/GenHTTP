using System.Reflection;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Controllers.Provider;

public sealed partial class ControllerHandler : IHandler, IServiceMethodProvider
{
    private static readonly Regex HyphenMatcher = CreateHyphenMatcher();

    #region Get-/Setters

    public MethodCollection Methods { get; }

    #endregion

    #region Initialization

    public ControllerHandler(Type type, Func<IRequest, ValueTask<object>> instanceProvider, MethodRegistry registry)
    {
        Methods = new MethodCollection(AnalyzeMethods(type, instanceProvider, registry));
    }

    private IEnumerable<MethodHandler> AnalyzeMethods(Type type, Func<IRequest, ValueTask<object>> instanceProvider, MethodRegistry registry)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var annotation = method.GetCustomAttribute<ControllerActionAttribute>(true) ?? new MethodAttribute();

            var arguments = FindPathArguments(method);

            var operation = CreateOperation(method, arguments, registry);

            yield return new MethodHandler(operation, instanceProvider, annotation, registry);
        }
    }

    private Operation CreateOperation(MethodInfo method, List<string> arguments, MethodRegistry registry)
    {
        var pathArguments = string.Join('/', arguments.Select(a => $":{a}"));

        if (method.Name == "Index")
        {
            return OperationBuilder.Create(pathArguments.Length > 0 ? $"/{pathArguments}/" : null, method, registry, true);
        }

        var name = HypenCase(method.Name);

        var path = $"/{name}";

        return OperationBuilder.Create(pathArguments.Length > 0 ? $"{path}/{pathArguments}/" : $"{path}/", method, registry, true);
    }

    private static List<string> FindPathArguments(MethodInfo method)
    {
        var found = new List<string>();

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.Name != null)
            {
                if (parameter.GetCustomAttribute<FromPathAttribute>(true) is not null)
                {
                    found.Add(parameter.Name);
                }
            }
        }

        return found;
    }

    private static string HypenCase(string input) => HyphenMatcher.Replace(input, "$1-$2").ToLowerInvariant();

    [GeneratedRegex("([a-z])([A-Z0-9]+)")]
    private static partial Regex CreateHyphenMatcher();

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
