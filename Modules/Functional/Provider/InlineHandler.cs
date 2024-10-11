using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Functional.Provider;

public class InlineHandler : IHandler
{

    #region Get-/Setters

    public IHandler Parent { get; }

    private MethodCollection Methods { get; }

    private ResponseProvider ResponseProvider { get; }

    #endregion

    #region Initialization

    public InlineHandler(IHandler parent, List<InlineFunction> functions, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
    {
        Parent = parent;

        ResponseProvider = new(serialization, formatting);

        Methods = new(this, AnalyzeMethods(functions, serialization, injection, formatting));
    }

    private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(List<InlineFunction> functions, SerializationRegistry formats, InjectionRegistry injection, FormatterRegistry formatting)
    {
        foreach (var function in functions)
        {
            var method = function.Delegate.Method;

            var wildcardRoute = PathArguments.CheckWildcardRoute(method.ReturnType);

            var path = PathArguments.Route(function.Path, wildcardRoute);

            var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

            yield return (parent) => new MethodHandler(parent, method, path, () => target, function.Configuration, ResponseProvider.GetResponseAsync, formats, injection, formatting);
        }
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
