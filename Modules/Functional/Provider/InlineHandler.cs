using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Functional.Provider;

public class InlineHandler : IHandler, IServiceMethodProvider
{

    #region Get-/Setters

    private List<InlineFunction> Functions { get; }

    private MethodRegistry Registry { get; }

    private ExecutionSettings ExecutionSettings { get; }

    public MethodCollectionFactory Methods { get; }

    #endregion

    #region Initialization

    public InlineHandler(List<InlineFunction> functions, MethodRegistry registry, ExecutionSettings executionSettings)
    {
        Functions = functions;
        Registry = registry;
        ExecutionSettings = executionSettings;

        Methods = new MethodCollectionFactory(GetMethodsAsync);
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request) => await (await Methods.GetAsync(request)).HandleAsync(request);

    private async Task<MethodCollection> GetMethodsAsync(IRequest request)
    {
        var found = new List<MethodHandler>();

        foreach (var function in Functions)
        {
            var method = function.Delegate.Method;

            var operation = OperationBuilder.Create(request, function.Path, method, function.Delegate, ExecutionSettings, Registry);

            var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

            var instanceProvider = (IRequest _) => ValueTask.FromResult(target);

            found.Add(new MethodHandler(operation, instanceProvider, function.Configuration, Registry));
        }

        var result = new MethodCollection(found);

        await result.PrepareAsync();

        return result;
    }

    #endregion

}
