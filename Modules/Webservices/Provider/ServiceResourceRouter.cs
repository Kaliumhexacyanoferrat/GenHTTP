using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Webservices.Provider;

public sealed class ServiceResourceRouter : IHandler, IServiceMethodProvider
{
    private MethodCollection? _methodCache;

    #region Get-/Setters

    private Type Type { get; }

    private Func<IRequest, ValueTask<object>> InstanceProvider { get; }

    private MethodRegistry Registry { get; }

    private ExecutionSettings ExecutionSettings { get; }

    public MethodCollectionFactory Methods { get; }

    #endregion

    #region Initialization

    public ServiceResourceRouter(Type type, Func<IRequest, ValueTask<object>> instanceProvider, ExecutionSettings executionSettings, MethodRegistry registry)
    {
        Type = type;
        InstanceProvider = instanceProvider;
        ExecutionSettings = executionSettings;
        Registry = registry;

        Methods = new MethodCollectionFactory(GetMethodsAsync);
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (_methodCache == null)
        {
            return HandleWithInitialization(request);
        }

        return _methodCache.HandleAsync(request);
    }

    private async ValueTask<IResponse?> HandleWithInitialization(IRequest request)
    {
        _methodCache = await Methods.GetAsync(request);

        return await _methodCache.HandleAsync(request);
    }

    private async Task<MethodCollection> GetMethodsAsync(IRequest request)
    {
        var found = new List<MethodHandler>();

        foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

            if (attribute is not null)
            {
                var operation = OperationBuilder.Create(request, attribute.Path, method, null, ExecutionSettings, Registry);

                found.Add(new MethodHandler(operation, InstanceProvider, attribute, Registry));
            }
        }

        var result = new MethodCollection(found);

        await result.PrepareAsync();

        return result;
    }

    #endregion

}
