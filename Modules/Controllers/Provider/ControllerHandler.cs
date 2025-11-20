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

    private MethodCollection? _methods;

    #region Get-/Setters

    private Type Type { get; }

    private Func<IRequest, ValueTask<object>> InstanceProvider { get; }

    private MethodRegistry Registry { get; }

    #endregion

    #region Initialization

    public ControllerHandler(Type type, Func<IRequest, ValueTask<object>> instanceProvider, MethodRegistry registry)
    {
        Type = type;
        InstanceProvider = instanceProvider;
        Registry = registry;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request) => await (await GetMethodsAsync(request)).HandleAsync(request);

    public async ValueTask<MethodCollection> GetMethodsAsync(IRequest request)
    {
        if (_methods != null) return _methods;

        var found = new List<MethodHandler>();


        foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var annotation = method.GetCustomAttribute<ControllerActionAttribute>(true) ?? new MethodAttribute();

            var arguments = FindPathArguments(method);

            var operation = CreateOperation(request, method, arguments, Registry);

            found.Add(new MethodHandler(operation, InstanceProvider, annotation, Registry));
        }

        var result = new MethodCollection(found);

        await result.PrepareAsync();

        return _methods = result;
    }

    private static Operation CreateOperation(IRequest request, MethodInfo method, List<string> arguments, MethodRegistry registry)
    {
        var pathArguments = string.Join('/', arguments.Select(a => $":{a}"));

        if (method.Name == "Index")
        {
            return OperationBuilder.Create(request, pathArguments.Length > 0 ? $"/{pathArguments}/" : null, method, registry, true);
        }

        var name = HypenCase(method.Name);

        var path = $"/{name}";

        return OperationBuilder.Create(request, pathArguments.Length > 0 ? $"{path}/{pathArguments}/" : $"{path}/", method, registry, true);
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

}
