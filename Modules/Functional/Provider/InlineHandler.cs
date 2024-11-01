using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Functional.Provider;

public class InlineHandler : IHandler, IServiceMethodProvider
{

    #region Get-/Setters

    public MethodCollection Methods { get; }

    private ResponseProvider ResponseProvider { get; }

    #endregion

    #region Initialization

    public InlineHandler(List<InlineFunction> functions, MethodRegistry registry)
    {
        ResponseProvider = new ResponseProvider(registry);

        Methods = new MethodCollection(AnalyzeMethods(functions, registry));
    }

    private static IEnumerable<MethodHandler> AnalyzeMethods(List<InlineFunction> functions, MethodRegistry registry)
    {
        foreach (var function in functions)
        {
            var method = function.Delegate.Method;

            var operation = OperationBuilder.Create(function.Path, method, registry);

            var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

            yield return new MethodHandler(operation, target, function.Configuration, registry);
        }
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
