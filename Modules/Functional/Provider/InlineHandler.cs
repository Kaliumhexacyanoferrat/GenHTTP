using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Functional.Provider;

public sealed class InlineHandler : IHandler, IServiceMethodProvider
{
    private MethodCollection? _methods;

    #region Get-/Setters

    private List<InlineFunction> Functions { get; }

    private MethodRegistry Registry { get; }

    private ExecutionSettings ExecutionSettings { get; }

    public MethodCollection Methods => _methods ?? throw new InvalidOperationException("Handler is not prepared yet");

    #endregion

    #region Initialization

    public InlineHandler(List<InlineFunction> functions, MethodRegistry registry, ExecutionSettings executionSettings)
    {
        Functions = functions;
        Registry = registry;
        ExecutionSettings = executionSettings;
    }

    #endregion

    #region Functionality

    public async ValueTask PrepareAsync(IServer server)
    {
        var found = new List<MethodHandler>();

        foreach (var function in Functions)
        {
            var method = function.Delegate.Method;

            var operation = OperationBuilder.Create(server, function.Path, method, function.Delegate, ExecutionSettings, function.Configuration, Registry);

            var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

            var instanceProvider = (IRequest _) => ValueTask.FromResult(target);

            found.Add(new MethodHandler(operation, instanceProvider, Registry));
        }

        var result = new MethodCollection(found);

        await result.PrepareAsync(server);

        _methods = result;
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
