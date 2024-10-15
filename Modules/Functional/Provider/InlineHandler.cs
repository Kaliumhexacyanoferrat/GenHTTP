using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Functional.Provider;

public class InlineHandler : IHandler
{

    #region Get-/Setters

    public IHandler Parent { get; }

    public MethodCollection Methods { get; }

    private ResponseProvider ResponseProvider { get; }

    #endregion

    #region Initialization

    public InlineHandler(IHandler parent, List<InlineFunction> functions, MethodExtensions extensions)
    {
        Parent = parent;

        ResponseProvider = new ResponseProvider(extensions);

        Methods = new MethodCollection(this, AnalyzeMethods(functions, extensions));
    }

    private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(List<InlineFunction> functions, MethodExtensions extensions)
    {
        foreach (var function in functions)
        {
            var method = function.Delegate.Method;

            var operation = OperationBuilder.Create(function.Path, method, extensions);

            var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

            yield return parent => new MethodHandler(parent, operation, () => target, function.Configuration, ResponseProvider.GetResponseAsync, extensions);
        }
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
